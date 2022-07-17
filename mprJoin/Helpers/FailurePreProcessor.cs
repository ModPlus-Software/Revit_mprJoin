namespace mprJoin.Helpers
{
    using System.Linq;
    using Autodesk.Revit.DB;

    public class FailurePreProcessor : IFailuresPreprocessor
    {
        /// <inheridoc />
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            var transactionName = failuresAccessor.GetTransactionName();
            if (!(transactionName == ModPlusAPI.Language.GetItem("t3")
                || transactionName == ModPlusAPI.Language.GetItem("t2")))
                return FailureProcessingResult.Continue;

            var fmas = failuresAccessor.GetFailureMessages();
            if (!fmas.Any())
                return FailureProcessingResult.Continue;

            failuresAccessor.ResolveFailures(fmas);

            return FailureProcessingResult.ProceedWithCommit;
        }
    }
}
