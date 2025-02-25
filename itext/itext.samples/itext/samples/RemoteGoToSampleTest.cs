using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using iText.Commons.Utils;
using iText.IO.Font;
using iText.Kernel.Utils;
using iText.Licensing.Base;
using iText.Licensing.Base.Reporting;
using iText.Test;
using NUnit.Framework;

namespace iText.Samples
{
    [TestFixtureSource("Data")]
    public class RemoteGoToSampleTest : WrappedSamplesRunner
    {
        public RemoteGoToSampleTest(RunnerParams runnerParams) : base(runnerParams)
        {
        }

        public static ICollection<TestFixtureData> Data()
        {
            RunnerSearchConfig searchConfig = new RunnerSearchConfig();
            searchConfig.AddClassToRunnerSearchPath("iText.Samples.Sandbox.Annotations.RemoteGoto");
            searchConfig.AddClassToRunnerSearchPath("iText.Samples.Sandbox.Annotations.RemoteGoToPage");

            return GenerateTestsList(Assembly.GetExecutingAssembly(), searchConfig);
        }

        [Timeout(60000)]
        [Test, Description("{0}")]
        public virtual void Test()
        {
            LicenseKeyReportingConfigurer.UseLocalReporting("./target/test/com/itextpdf/samples/report/");
            using (Stream license = FileUtil.GetInputStreamForFile(
                Environment.GetEnvironmentVariable("ITEXT7_LICENSEKEY") + "/all-products.json"))
            {
                LicenseKey.LoadLicenseFile(license);
            }
            FontCache.ClearSavedFonts();
            FontProgramFactory.ClearRegisteredFonts();

            RunSamples();
            LicenseKey.UnloadLicenses();
        }

        protected override void ComparePdf(string outPath, string dest, string cmp)
        {
            CompareTool compareTool = new CompareTool();
            String[] names = GetDestNames(sampleClass);

            foreach (String fileName in names)
            {
                String currentDest = dest + fileName;
                String temp = cmp + fileName;
                int i = temp.LastIndexOf("/");
                String currentCmp = temp.Substring(0, i + 1) + "cmp_" + temp.Substring(i + 1);

                AddError(compareTool.CompareByContent(currentDest, currentCmp, outPath, "diff_"));
                AddError(compareTool.CompareDocumentInfo(currentDest, currentCmp));
            }
        }

        protected override String GetCmpPdf(String dest)
        {
            if (dest == null)
            {
                return null;
            }

            int j = dest.LastIndexOf("/results", StringComparison.Ordinal) + 9;
            return "../../../cmpfiles/" + dest.Substring(j);
        }

        private static String[] GetDestNames(Type c)
        {
            try
            {
                FieldInfo field = c.GetField("DEST_NAMES");
                if (field == null)
                {
                    return null;
                }

                Object obj = field.GetValue(null);
                if (obj == null || !(obj is String[]))
                {
                    return null;
                }

                return (String[]) obj;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
