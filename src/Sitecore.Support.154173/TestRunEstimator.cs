using Sitecore.ContentTesting.Data;
using Sitecore.ContentTesting.Intelligence;
using Sitecore.ContentTesting.Intelligence.Pipelines.ForecastTestDuration;
using Sitecore.ContentTesting.Model.Data.Items;
using Sitecore.Data;
using System;

namespace Sitecore.Support.ContentTesting.Data
{
    public class TestRunEstimator: Sitecore.ContentTesting.Data.TestRunEstimator
    {
        public TestRunEstimator(string language, string deviceName) : base(language, deviceName)
        {
        }

        protected override TestRunEstimate GetRawEstimate(int experienceCount, double requiredPower, double trafficAllocation, double confidenceLevel, TestDefinitionItem testDefinition, TestMeasurement measurement = TestMeasurement.Undefined)
        {
            ID goalID = null;

            if (testDefinition != null)
            {
                ID.TryParse(testDefinition.Conversion, out goalID);
            }

            var measurementToUse = measurement;
            if (measurementToUse == TestMeasurement.Undefined)
            {
                measurementToUse = goalID == (ID)null ? TestMeasurement.TrailingValue : TestMeasurement.GoalConversion;
            }

            var args = new ForecastTestDurationPipelineArgs(
              this.HostItem,
              this.language,
              this.deviceName,
              requiredPower,
              experienceCount,
              trafficAllocation,
              confidenceLevel,
              measurementToUse);

            ForecastTestDurationPipeline.Run(args);

            var result = new TestRunEstimate
            {
                RequiredSessionCount = args.SessionCountRequired
            };

            if (args.DurationDays >= 0)
            {
                result.EstimatedDayCount = args.DurationDays;
            }

            if (args.VisitsPerDay >= 0)
            {
                result.VisitsPerDay = Math.Round(args.VisitsPerDay, 2);
            }
            //prevent EstimatedDayCount exceeds maximum date
            var maximumDaysToRunTest = System.Convert.ToInt32((DateTime.MaxValue - testDefinition.StartDate).TotalDays);
            if (result.EstimatedDayCount > maximumDaysToRunTest)
                result.EstimatedDayCount = maximumDaysToRunTest - 1;

            return result;
        }
    }
}