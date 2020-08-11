using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class MsbuildConditionEvaluator
    {

        public static string[] EvaluateTargetFrameworks(string csprojPath, string condition)
        {
            if (!condition.Contains("$(TargetFramework)"))
            {
                Log.WriteLine($"{csprojPath}: condition {condition} skipped");
                return null;
            }

            var conditionClauses = Regex.Split(condition, "^or$");

            var targetFrameworks = conditionClauses
                .Select(clause => EvaluateClause(csprojPath, clause))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();

            Log.WriteLine($"{csprojPath}: condition {condition} evaluates into [{string.Join(",", targetFrameworks)}]");
            return targetFrameworks;
        }

        private static string EvaluateClause(string csprojPath, string clause)
        {
            if (string.IsNullOrEmpty(clause))
            {
                return null;
            }

            var m = Regex.Match(clause, @"\s*(|')\$\(TargetFramework\)(|')\s*==\s*'([^']+)'s*");
            if (!m.Success)
            {
                Log.WriteLine(
                    $"{csprojPath}: clause {clause} is not supported"
                );
                throw new Exception($"MSBuild condition is not supported: \"{clause}\"");
            }

            return m.Groups[3].Value;
        }

    }
}