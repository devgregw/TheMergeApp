using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MergeApi.Framework.Enumerations;
using Newtonsoft.Json;

namespace MergeApi.Tools {
    public sealed class RecurrenceRule {
        [JsonProperty("frequency")]
        public RecurrenceFrequency Frequency { get; set; }

        [JsonProperty("interval")]
        public int Interval { get; set; }

        [JsonProperty("end")]
        public DateTime? End { get; set; }

        [JsonProperty("count")]
        public int? Count { get; set; }

        public override string ToString() {
            var main = $"Repeats every{(Interval == 1 ? "" : Interval == 2 ? " other" : $" {Interval}")} {Frequency.ToString().ToLower().Replace("ly", "")}{(Interval <= 2 ? "" : "s")}";
            if (End.HasValue)
                main +=
                    $" until {End.Value.ToString("D", DateTimeFormatInfo.CurrentInfo)} {End.Value.ToString("h:mm tt", CultureInfo.CurrentUICulture)}";
            else if (Count.HasValue) {
                var remaining = GetAllOccurrences(DateTime.Today, this).Count;
                if (remaining > 0)
                    main += $" {remaining} more time{(remaining == 1 ? "" : "s")}";
            }
            return main;
        }

        public static List<DateTime> GetAllOccurrences(DateTime initial, RecurrenceRule rule) {
            var dates = new List<DateTime> {
                initial
            };
            if (rule.End.HasValue) {
                while (dates.Max() < rule.End.Value)
                    dates.Add(InternalGetNextOccurrence(dates.Max(), rule));
                if (dates.Max() > rule.End.Value)
                    dates.Remove(dates.Max());
            } else if (rule.Count.HasValue)
                while (dates.Count < rule.Count.Value)
                    dates.Add(InternalGetNextOccurrence(dates.Max(), rule));
            else // It repeats infinitely, so we return the next 30 occurrences
                return GetAllOccurrences(initial, new RecurrenceRule {
                    Frequency = rule.Frequency,
                    Interval = rule.Interval,
                    End = null,
                    Count = 30
                });
            return dates;
        }

        private static DateTime InternalGetNextOccurrence(DateTime initial, RecurrenceRule rule) {
            DateTime next;
            switch (rule.Frequency) {
                case RecurrenceFrequency.Daily:
                    next = initial.AddDays(rule.Interval);
                    break;
                case RecurrenceFrequency.Weekly:
                    next = initial.AddDays(7 * rule.Interval);
                    break;
                case RecurrenceFrequency.Monthly:
                    next = initial.AddMonths(rule.Interval);
                    break;
                case RecurrenceFrequency.Yearly:
                    next = initial.AddYears(rule.Interval);
                    break;
                default:
                    next = initial;
                    break;
            }
            return next;
        }

        public static DateTime? GetNextOccurrence(DateTime initial, RecurrenceRule rule) {
            try {
                return GetAllOccurrences(initial, rule).First(d => d >= DateTime.Now);
            } catch {
                return null;
            }
        }
    }
}
