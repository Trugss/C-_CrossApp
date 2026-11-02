namespace Core.Reports;

public sealed record BenchmarkResult(string Variant, double Milliseconds, int Rows);
