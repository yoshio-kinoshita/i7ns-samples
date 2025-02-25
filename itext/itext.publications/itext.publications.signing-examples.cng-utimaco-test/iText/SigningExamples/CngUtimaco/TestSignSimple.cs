﻿using iText.Kernel.Pdf;
using iText.Signatures;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.X509;
using BcX509 = Org.BouncyCastle.X509;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using static iText.Signatures.PdfSigner;
using iText.SigningExamples.Simple;
using System.Security.Cryptography.Pkcs;
using iText.Bouncycastle.Cert;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1;

namespace iText.SigningExamples.CngUtimaco
{
    class TestSignSimple
    {
        [SetUp]
        public void Init()
        {
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadWrite))
            {
                X509Certificate2Collection certificates = store.Certificates;
                X509Certificate2Collection ecdsaSigningCertificates = certificates.Find(X509FindType.FindBySubjectName, "Utimaco CNG ECDSA Signing Test", false);
                if (ecdsaSigningCertificates.Count == 0)
                {
                    CngProvider provider = new CngProvider("Utimaco CryptoServer Key Storage Provider");
                    CngKey key = CngKey.Open("DEMOecdsaKEY", provider);
                    ECDsaCng ecdsaKey = new ECDsaCng(key);
                    CertificateRequest request = new CertificateRequest("CN = Utimaco CNG ECDSA Signing Test", ecdsaKey, HashAlgorithmName.SHA512);
                    X509Certificate2 certificate = request.CreateSelfSigned(System.DateTimeOffset.Now, System.DateTimeOffset.Now.AddYears(2));
                    certificate.FriendlyName = "Utimaco CNG ECDSA Signing Test";
                    System.Console.WriteLine("Utimaco CNG ECDSA Signing Test Certificate generated:\n****\n{0}\n****", certificate);
                    store.Add(certificate);
                }

                X509Certificate2Collection rsaSigningCertificates = certificates.Find(X509FindType.FindBySubjectName, "Utimaco CNG RSA Signing Test", false);
                if (rsaSigningCertificates.Count == 0)
                {
                    CngProvider provider = new CngProvider("Utimaco CryptoServer Key Storage Provider");
                    CngKey key = CngKey.Open("DEMOrsaKEY", provider);
                    RSACng rsaKey = new RSACng(key);
                    CertificateRequest request = new CertificateRequest("CN = Utimaco CNG RSA Signing Test", rsaKey, HashAlgorithmName.SHA512, RSASignaturePadding.Pss);
                    X509Certificate2 certificate = request.CreateSelfSigned(System.DateTimeOffset.Now, System.DateTimeOffset.Now.AddYears(2));
                    certificate.FriendlyName = "Utimaco CNG RSA Signing Test";
                    System.Console.WriteLine("Utimaco CNG RSA Signing Test Certificate generated:\n****\n{0}\n****", certificate);
                    store.Add(certificate);
                }
            }
        }

        [Test]
        public void TestCngSignEcdsaSimple()
        {
            string testFileName = @"..\..\..\resources\circles.pdf";

            X509Certificate2 certificate;
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly))
            {
                X509Certificate2Collection certificates = store.Certificates;
                X509Certificate2Collection signingcertificates = certificates.Find(X509FindType.FindBySubjectName, "Utimaco CNG ECDSA Signing Test", false);
                certificate = signingcertificates[0];
            }

            using (PdfReader pdfReader = new PdfReader(testFileName))
            using (FileStream result = File.Create("circles-cng-signed-ecdsa-simple-custom.pdf"))
            {
                PdfSigner pdfSigner = new PdfSigner(pdfReader, result, new StampingProperties().UseAppendMode());
                X509Certificate2ECDsaSignature signature = new X509Certificate2ECDsaSignature(certificate);
                IX509Certificate[] certificateWrappers = new IX509Certificate[signature.GetChain().Length];
                for (int i = 0; i < certificateWrappers.Length; ++i) {
                    certificateWrappers[i] = new X509CertificateBC(signature.GetChain()[i]);
                }

                pdfSigner.SignDetached(signature, certificateWrappers, null, null, null, 0, CryptoStandard.CMS);
            }
        }

        [Test]
        public void TestCngSignEcdsaSimpleGeneric()
        {
            string testFileName = @"..\..\..\resources\circles.pdf";

            X509Certificate2 certificate;
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly))
            {
                X509Certificate2Collection certificates = store.Certificates;
                X509Certificate2Collection signingcertificates = certificates.Find(X509FindType.FindBySubjectName, "Utimaco CNG ECDSA Signing Test", false);
                certificate = signingcertificates[0];
            }

            BcX509.X509Certificate bcCertificate = new BcX509.X509Certificate(X509CertificateStructure.GetInstance(certificate.RawData));
            IX509Certificate[] chain = { new X509CertificateBC(bcCertificate) };

            using (PdfReader pdfReader = new PdfReader(testFileName))
            using (FileStream result = File.Create("circles-cng-signed-ecdsa-simple-generic.pdf"))
            {
                PdfSigner pdfSigner = new PdfSigner(pdfReader, result, new StampingProperties().UseAppendMode());
                X509Certificate2Signature signature = new X509Certificate2Signature(certificate, "SHA512");

                pdfSigner.SignDetached(signature, chain, null, null, null, 0, CryptoStandard.CMS);
            }
        }

        [Test]
        public void TestCngSignRsaSimpleGeneric()
        {
            string testFileName = @"..\..\..\resources\circles.pdf";

            X509Certificate2 certificate;
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly))
            {
                X509Certificate2Collection certificates = store.Certificates;
                X509Certificate2Collection signingcertificates = certificates.Find(X509FindType.FindBySubjectName, "Utimaco CNG RSA Signing Test", false);
                certificate = signingcertificates[0];
            }

            BcX509.X509Certificate bcCertificate = new BcX509.X509Certificate(X509CertificateStructure.GetInstance(certificate.RawData));
            IX509Certificate[] chain = { new X509CertificateBC(bcCertificate) };

            using (PdfReader pdfReader = new PdfReader(testFileName))
            using (FileStream result = File.Create("circles-cng-signed-rsa-simple-generic.pdf"))
            {
                PdfSigner pdfSigner = new PdfSigner(pdfReader, result, new StampingProperties().UseAppendMode());
                X509Certificate2Signature signature = new X509Certificate2Signature(certificate, "SHA512");

                pdfSigner.SignDetached(signature, chain, null, null, null, 0, CryptoStandard.CMS);
            }
        }

        [Test]
        public void TestCngSignRsaSsaPssSimpleGeneric()
        {
            string testFileName = @"..\..\..\resources\circles.pdf";

            X509Certificate2 certificate;
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly))
            {
                X509Certificate2Collection certificates = store.Certificates;
                X509Certificate2Collection signingcertificates = certificates.Find(X509FindType.FindBySubjectName, "Utimaco CNG RSA Signing Test", false);
                certificate = signingcertificates[0];
            }

            BcX509.X509Certificate bcCertificate = new BcX509.X509Certificate(X509CertificateStructure.GetInstance(certificate.RawData));
            IX509Certificate[] chain = { new X509CertificateBC(bcCertificate) };

            using (PdfReader pdfReader = new PdfReader(testFileName))
            using (FileStream result = File.Create("circles-cng-signed-rsassapss-simple-generic.pdf"))
            {
                PdfSigner pdfSigner = new PdfSigner(pdfReader, result, new StampingProperties().UseAppendMode());
                X509Certificate2Signature signature = new X509Certificate2Signature(certificate, "SHA512");
                signature.UsePssForRsaSsa = true;

                pdfSigner.SignDetached(signature, chain, null, null, null, 0, CryptoStandard.CMS);
            }
        }

        [Test]
        public void TestCngSignEcdsaSimpleGenericContainer()
        {
            string testFileName = @"..\..\..\resources\circles.pdf";

            X509Certificate2 certificate;
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly))
            {
                X509Certificate2Collection certificates = store.Certificates;
                X509Certificate2Collection signingcertificates = certificates.Find(X509FindType.FindBySubjectName, "Utimaco CNG ECDSA Signing Test", false);
                certificate = signingcertificates[0];
            }

            X509Certificate2SignatureContainer signature = new X509Certificate2SignatureContainer(certificate, signer => {
                signer.DigestAlgorithm = Oid.FromFriendlyName("SHA512", OidGroup.HashAlgorithm);
                signer.SignedAttributes.Add(new Pkcs9SigningTime());
            });

            using (PdfReader pdfReader = new PdfReader(testFileName))
            using (FileStream result = File.Create("circles-cng-signed-ecdsa-simple-generic-container.pdf"))
            {
                PdfSigner pdfSigner = new PdfSigner(pdfReader, result, new StampingProperties().UseAppendMode());

                pdfSigner.SignExternalContainer(signature, 8192);
            }
        }
    }

    class X509Certificate2ECDsaSignature : IExternalSignature
    {
        public X509Certificate2ECDsaSignature(X509Certificate2 certificate)
        {
            this.certificate = certificate;
        }

        public Org.BouncyCastle.X509.X509Certificate[] GetChain()
        {
            var bcCertificate = new Org.BouncyCastle.X509.X509Certificate(Org.BouncyCastle.Asn1.X509.X509CertificateStructure.GetInstance(certificate.RawData));
            return new Org.BouncyCastle.X509.X509Certificate[] { bcCertificate };
        }

        public string GetSignatureAlgorithmName()
        {
            return "ECDSA";
        }

        public ISignatureMechanismParams GetSignatureMechanismParameters()
        {
            return null;
        }

        public string GetDigestAlgorithmName()
        {
            return "SHA512";
        }

        public byte[] Sign(byte[] message)
        {
            using (ECDsa ecdsa = certificate.GetECDsaPrivateKey())
            {
                return PlainToDer(ecdsa.SignData(message, HashAlgorithmName.SHA512));
            }
        }

        byte[] PlainToDer(byte[] plain)
        {
            int valueLength = plain.Length / 2;
            BigInteger r = new BigInteger(1, plain, 0, valueLength);
            BigInteger s = new BigInteger(1, plain, valueLength, valueLength);
            return new DerSequence(new DerInteger(r), new DerInteger(s)).GetEncoded(Asn1Encodable.Der);
        }

        X509Certificate2 certificate;
    }
}
