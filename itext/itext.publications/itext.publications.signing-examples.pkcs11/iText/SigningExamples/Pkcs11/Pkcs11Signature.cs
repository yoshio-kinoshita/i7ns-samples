﻿using iText.Signatures;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.Factories;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;

namespace iText.SigningExamples.Pkcs11
{
    public class Pkcs11Signature : IExternalSignature, IDisposable
    {
        IPkcs11Library pkcs11Library;
        ISlot slot;
        ISession session;
        IObjectHandle privateKeyHandle;

        string alias;
        X509Certificate[] chain;
        string signatureAlgorithmName;
        string digestAlgorithmName;

        public Pkcs11Signature (string libraryPath, ulong slotId)
        {
            Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
            pkcs11Library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories, libraryPath, AppType.MultiThreaded);
            slot = pkcs11Library.GetSlotList(SlotsType.WithOrWithoutTokenPresent).Find(slot => slot.SlotId == slotId);
        }

        public Pkcs11Signature Select(string alias, string certLabel, string pin)
        {
            List<CKA> pkAttributeKeys = new List<CKA>();
            pkAttributeKeys.Add(CKA.CKA_KEY_TYPE);
            pkAttributeKeys.Add(CKA.CKA_LABEL);
            pkAttributeKeys.Add(CKA.CKA_ID);
            List<CKA> certAttributeKeys = new List<CKA>();
            certAttributeKeys.Add(CKA.CKA_VALUE);
            certAttributeKeys.Add(CKA.CKA_LABEL);

            CloseSession();
            session = slot.OpenSession(SessionType.ReadWrite);
            session.Login(CKU.CKU_USER, pin);
            ObjectAttributeFactory objectAttributeFactory = new ObjectAttributeFactory();

            List<IObjectAttribute> attributes = new List<IObjectAttribute>();
            attributes.Add(objectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY));
            List<IObjectHandle> keys = session.FindAllObjects(attributes);

            bool found = false;
            foreach (IObjectHandle key in keys)
            {
                List<IObjectAttribute> keyAttributes = session.GetAttributeValue(key, pkAttributeKeys);

                ulong type = keyAttributes[0].GetValueAsUlong();
                string signatureAlgorithmName;
                switch (type)
                {
                    case (ulong)CKK.CKK_RSA:
                        signatureAlgorithmName = "RSA";
                        break;
                    case (ulong)CKK.CKK_DSA:
                        signatureAlgorithmName = "DSA";
                        break;
                    case (ulong)CKK.CKK_ECDSA:
                        signatureAlgorithmName = "ECDSA";
                        break;
                    default:
                        continue;
                }
                string thisAlias = keyAttributes[1].GetValueAsString();
                if (thisAlias == null || thisAlias.Length == 0)
                    thisAlias = keyAttributes[2].GetValueAsString();
                if (alias != null && !alias.Equals(thisAlias))
                    continue;

                attributes.Clear();
                attributes.Add(objectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE));
                attributes.Add(objectAttributeFactory.Create(CKA.CKA_CERTIFICATE_TYPE, CKC.CKC_X_509));
                if (certLabel == null && thisAlias != null && thisAlias.Length > 0)
                    certLabel = thisAlias;
                if (certLabel != null)
                    attributes.Add(objectAttributeFactory.Create(CKA.CKA_LABEL, certLabel));
                List<IObjectHandle> certificates = session.FindAllObjects(attributes);
                if (certificates.Count != 1)
                    continue;

                IObjectHandle certificate = certificates[0];
                List<IObjectAttribute> certificateAttributes = session.GetAttributeValue(certificate, certAttributeKeys);
                X509Certificate x509Certificate = new X509Certificate(X509CertificateStructure.GetInstance(certificateAttributes[0].GetValueAsByteArray()));

                List<X509Certificate> x509Certificates = new List<X509Certificate>();
                x509Certificates.Add(x509Certificate);
                attributes.Clear();
                attributes.Add(objectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE));
                attributes.Add(objectAttributeFactory.Create(CKA.CKA_CERTIFICATE_TYPE, CKC.CKC_X_509));
                List<IObjectHandle> otherCertificates = session.FindAllObjects(attributes);
                foreach (IObjectHandle otherCertificate in otherCertificates)
                {
                    if (!certificate.ObjectId.Equals(otherCertificate.ObjectId))
                    {
                        certificateAttributes = session.GetAttributeValue(otherCertificate, certAttributeKeys);
                        X509Certificate otherX509Certificate = new X509Certificate(X509CertificateStructure.GetInstance(certificateAttributes[0].GetValueAsByteArray()));
                        x509Certificates.Add(otherX509Certificate);
                    }
                }

                found = true;
                this.alias = thisAlias;
                this.signatureAlgorithmName = signatureAlgorithmName;
                this.privateKeyHandle = key;
                this.chain = x509Certificates.ToArray();
                break;
            }

            if (!found)
            {
                this.alias = null;
                this.signatureAlgorithmName = null;
                this.privateKeyHandle = null;
                this.chain = null;
            }

            return this;
        }

        public void Dispose()
        {
            CloseSession();
            slot = null;
            pkcs11Library?.Dispose();
        }

        private void CloseSession()
        {
            if (session != null)
            {
                try
                {
                    session.Dispose();
                }
                finally
                {
                    privateKeyHandle = null;
                    session = null;
                }
            }
        }

        public X509Certificate[] GetChain()
        {
            return chain;
        }

        public bool UsePssForRsaSsa { get; set; }

        public string GetSignatureAlgorithmName()
        {
            return UsePssForRsaSsa && "RSA".Equals(signatureAlgorithmName) ? "RSASSA-PSS" : signatureAlgorithmName;
        }

        public ISignatureMechanismParams GetSignatureMechanismParameters()
        {
            return UsePssForRsaSsa && "RSA".Equals(signatureAlgorithmName) ? RSASSAPSSMechanismParams.CreateForDigestAlgorithm(digestAlgorithmName) : null;
        }

        public string GetDigestAlgorithmName()
        {
            return digestAlgorithmName;
        }

        public Pkcs11Signature SetDigestAlgorithmName(String digestAlgorithmName)
        {
            this.digestAlgorithmName = DigestAlgorithms.GetDigest(DigestAlgorithms.GetAllowedDigest(digestAlgorithmName));
            return this;
        }

        public byte[] Sign(byte[] message)
        {
            MechanismFactory mechanismFactory = new MechanismFactory();
            IMechanism mechanism;

            switch(signatureAlgorithmName)
            {
                case "DSA":
                    switch(digestAlgorithmName)
                    {
                        case "SHA1":
                            mechanism = mechanismFactory.Create(CKM.CKM_DSA_SHA1);
                            break;
                        case "SHA224":
                            mechanism = mechanismFactory.Create(CKM.CKM_DSA_SHA224);
                            break;
                        case "SHA256":
                            mechanism = mechanismFactory.Create(CKM.CKM_DSA_SHA256);
                            break;
                        case "SHA384":
                            mechanism = mechanismFactory.Create(CKM.CKM_DSA_SHA384);
                            break;
                        case "SHA512":
                            mechanism = mechanismFactory.Create(CKM.CKM_DSA_SHA512);
                            break;
                        default:
                            throw new ArgumentException("Not supported: " + digestAlgorithmName + "with" + signatureAlgorithmName);
                    }
                    break;
                case "ECDSA":
                    switch (digestAlgorithmName)
                    {
                        case "SHA1":
                            mechanism = mechanismFactory.Create(CKM.CKM_ECDSA_SHA1);
                            break;
                        case "SHA224":
                            mechanism = mechanismFactory.Create(CKM.CKM_ECDSA_SHA224);
                            break;
                        case "SHA256":
                            mechanism = mechanismFactory.Create(CKM.CKM_ECDSA_SHA256);
                            break;
                        case "SHA384":
                            mechanism = mechanismFactory.Create(CKM.CKM_ECDSA_SHA384);
                            break;
                        case "SHA512":
                            mechanism = mechanismFactory.Create(CKM.CKM_ECDSA_SHA512);
                            break;
                        default:
                            throw new ArgumentException("Not supported: " + digestAlgorithmName + "with" + signatureAlgorithmName);
                    }
                    break;
                case "RSA":
                    if (UsePssForRsaSsa)
                    {
                        MechanismParamsFactory mechanismParamsFactory = new MechanismParamsFactory();
                        IMechanismParams pssParams = null;
                        switch (digestAlgorithmName)
                        {
                            case "SHA1":
                                pssParams = mechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM.CKM_SHA_1, (ulong)CKG.CKG_MGF1_SHA1, (ulong)(DigestAlgorithms.GetOutputBitLength(digestAlgorithmName) / 8));
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA1_RSA_PKCS_PSS, pssParams);
                                break;
                            case "SHA224":
                                pssParams = mechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM.CKM_SHA224, (ulong)CKG.CKG_MGF1_SHA224, (ulong)(DigestAlgorithms.GetOutputBitLength(digestAlgorithmName) / 8));
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA224_RSA_PKCS_PSS, pssParams);
                                break;
                            case "SHA256":
                                pssParams = mechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM.CKM_SHA256, (ulong)CKG.CKG_MGF1_SHA256, (ulong)(DigestAlgorithms.GetOutputBitLength(digestAlgorithmName) / 8));
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA256_RSA_PKCS_PSS, pssParams);
                                break;
                            case "SHA384":
                                pssParams = mechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM.CKM_SHA384, (ulong)CKG.CKG_MGF1_SHA384, (ulong)(DigestAlgorithms.GetOutputBitLength(digestAlgorithmName) / 8));
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA384_RSA_PKCS_PSS, pssParams);
                                break;
                            case "SHA512":
                                pssParams = mechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM.CKM_SHA224, (ulong)CKG.CKG_MGF1_SHA224, (ulong)(DigestAlgorithms.GetOutputBitLength(digestAlgorithmName) / 8));
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA512_RSA_PKCS_PSS, pssParams);
                                break;
                            default:
                                throw new ArgumentException("Not supported: " + digestAlgorithmName + "with" + signatureAlgorithmName);
                        }
                    }
                    else
                    {
                        switch (digestAlgorithmName)
                        {
                            case "SHA1":
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA1_RSA_PKCS);
                                break;
                            case "SHA224":
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA224_RSA_PKCS);
                                break;
                            case "SHA256":
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA256_RSA_PKCS);
                                break;
                            case "SHA384":
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA384_RSA_PKCS);
                                break;
                            case "SHA512":
                                mechanism = mechanismFactory.Create(CKM.CKM_SHA512_RSA_PKCS);
                                break;
                            default:
                                throw new ArgumentException("Not supported: " + digestAlgorithmName + "with" + signatureAlgorithmName);
                        }
                    }
                    break;
                default:
                    throw new ArgumentException("Not supported: " + digestAlgorithmName + "with" + signatureAlgorithmName);
            }

            return session.Sign(mechanism, privateKeyHandle, message);
        }
    }
}
