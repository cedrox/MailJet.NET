﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailJet.Client.Response.Data;

namespace MailJet.Client.Tests
{
    [TestFixture]
    public class StatisticsTests
    {
        private MailJetClient _client;

        [SetUp]
        public void Setup()
        {
#if DEBUG
            var publicKey = Environment.GetEnvironmentVariable("MailJetPub", EnvironmentVariableTarget.User);
            var privateKey = Environment.GetEnvironmentVariable("MailJetPri", EnvironmentVariableTarget.User);
#else
            var publicKey = Environment.GetEnvironmentVariable("MailJetPub");
            var privateKey = Environment.GetEnvironmentVariable("MailJetPri");
#endif


            if (String.IsNullOrWhiteSpace(publicKey))
                throw new InvalidOperationException("Add your MailJet public API Key to the Environment Variable \"MailJetPub\".");
            if (String.IsNullOrWhiteSpace(privateKey))
                throw new InvalidOperationException("Add your MailJet private API Key to the Environment Variable \"MailJetPri\".");

            _client = new MailJetClient(publicKey, privateKey);
        }

      

        /// <summary>
        /// Get GraphStatistics day by day for a certain amount of time
        /// </summary>
        /// <remarks>
        /// No data if no activity.
        /// </remarks>
        [Test]
        [TestCase(10)]
        public void GetGraphStatistics(double numberOfDays = 1)
        {
            //-- Get last message campaign id
            //var message = _client.GetMessages().Data.First();
            var campaign = _client.GetCampaign(string.Format("TestApi_{0}-{1}", DateTime.Now.Year, DateTime.Now.Month));

            if (campaign.Data != null)
            {
                var beginDate = campaign.Data.First().CreatedAt;
                var endDate = campaign.Data.First().CreatedAt.AddDays(numberOfDays);
                
                //-- Request statistic Api from campaign id
                var result = _client.GetGraphStatistics(CampaignID: campaign.Data.First().ID, FromTS: beginDate, ToTS: endDate, Scale: "day"); //"7d"
                Assert.IsNotNull(result);

            }
            else
            {
                Assert.Fail("Please check that your send mail via Test method 'MailMessage_Html_Campaign*' before invoking this test method");
            }

        }
        

        /// <summary>
        /// Please prefer GraphStatistics
        /// The period of the aggregates (24 hours or 7 days). Allowed values: "7d" or "24h"
        /// </summary>
        /// <remarks>
        /// Use  Test Name:	MailMessage_Html_Campaign_... to get sent mail and have a sample data
        /// </remarks>
        [Test]
        public void GetAggregateGraphStatistics()
        {
            //-- Get last message campaign id
            //var message = _client.GetMessages().Data.First();
            var campaign = _client.GetCampaign(string.Format("TestApi_{0}-{1}", DateTime.Now.Year, DateTime.Now.Month));

            if (campaign.Data != null)
            {
                //-- Create CampaignAggregate from campaign id
                CampaignAggregate lRequestCampaignAggregate = new CampaignAggregate();
                lRequestCampaignAggregate.CampaignIDS = campaign.Data.First().ID.ToString();
                lRequestCampaignAggregate.Name = string.Format("Campaign-{0}", campaign.Data.First().ID);

                var lResponseCampaignAggregate = _client.CreateCampaignAggregates(lRequestCampaignAggregate);

                //-- Request statistic Api from campaign aggreate id
                var result = _client.GetAggregateGraphStatistics(Convert.ToInt32(lResponseCampaignAggregate.Data.First().ID), "24h"); //"7d"
                Assert.IsNotNull(result);

            }
            else
            {
                Assert.Fail("Please check that your send mail before invoking this test method");
            }

        }

    }
}
