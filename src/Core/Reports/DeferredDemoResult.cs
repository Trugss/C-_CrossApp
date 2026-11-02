namespace Core.Reports;

public sealed record DeferredDemoResult(int PredicateCallsAfterDeclare, int Before, int AfterAdd, int Snapshot, int QueryAfterClear, int TotalPredicateCalls);
