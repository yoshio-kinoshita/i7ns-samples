using System;
using System.Collections.Generic;
using System.Reflection;
using iText.Kernel.Utils;
using iText.Licensing.Base;
using iText.Test;
using NUnit.Framework;

namespace iText.Highlevel {
    [TestFixtureSource("Data")]
    public class HighLevelWrapperTest : WrappedSamplesRunner {
        public HighLevelWrapperTest(RunnerParams runnerParams) : base(runnerParams) {
        }
        public static ICollection<TestFixtureData> Data() {
            RunnerSearchConfig searchConfig = new RunnerSearchConfig();
            searchConfig.AddPackageToRunnerSearchPath("iText.Highlevel");
            
            searchConfig.IgnorePackageOrClass("iText.Highlevel.Chapter01.C01E05_Czech_Russian_Korean_Right");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.Chapter01.C01E06_Czech_Russian_Korean_Unicode");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.Chapter02.C02E15_ShowTextAlignedKerned");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.Chapter01.C01E02_Text_Paragraph_Cardo");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.Chapter01.C01E02_Text_Paragraph_Cardo2");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.Chapter01.C01E03_Text_Paragraph_NoCardo");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.Chapter07.C07E14_Encrypted");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.Notused");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.Util");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.HighLevelWrapperTest");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.HighLevelWrapperWithEncryptionTest");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.HighLevelWrapperWithLicenseTest");
            searchConfig.IgnorePackageOrClass("iText.Highlevel.HighLevelWrapperC01E02Cardo2Test");
            return GenerateTestsList(Assembly.GetExecutingAssembly(), searchConfig);
        }

        [NUnit.Framework.Timeout(60000)]
        [NUnit.Framework.Test, Description("{0}")]
        public virtual void Test() {
            LicenseKey.UnloadLicenses();
            RunSamples();
        }
		
		protected override string GetCmpPdf(String dest) {
            if (dest == null) {
                return null;
            }
            int i = dest.LastIndexOf("/");
            int j = dest.IndexOf("results") + 8;
            return "../../../cmpfiles/" + dest.Substring(j, (i + 1) - j) + "cmp_" + dest.Substring(i + 1);
        }
        
        protected override void ComparePdf(String outPath, String dest, String cmp) {
            CompareTool compareTool = new CompareTool();
            AddError(compareTool.CompareByContent(dest, cmp, outPath, "diff_"));
            AddError(compareTool.CompareDocumentInfo(dest, cmp));
        }
    }
}
