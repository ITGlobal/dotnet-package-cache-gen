using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class MsbuildConditionEvaluator
    {
        public static string[] EvaluateTargetFrameworks(string condition)
        {
            var conditionClauses = Regex.Split(condition, "^or$");

            var tf = conditionClauses
                .Select(EvaluateClause)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();
            return tf;
        }

        private static string EvaluateClause(string clause)
        {
            if (string.IsNullOrEmpty(clause))
            {
                return null;
            }

            var m = Regex.Match(clause, @"\s*(|')\$\(TargetFramework\)(|')\s*==\s*'([^']+)'s*");
            if (!m.Success)
            {
                Console.Error.WriteLine($"Warning: MSBuild condition is not supported: \"{clause}\"");
                return null;
            }

            return m.Groups[3].Value;
        }
    }
}