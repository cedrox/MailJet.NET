﻿using MailJet.Client.Response;
using MailJet.Client.Response.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;

namespace MailJet.Client.Tests
{
    [TestFixture]
    public class SendMail
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

        [Test]
        public void TemplateMessage()
        {
            var result = _client.SendTemplateMessage(26171,
                new MailAddress(ToAddress, "MailJet TO"),
                new MailAddress(FromAddress, "MailJet From"),
                "MailJet.NET Template TEST",
                new Dictionary<string, object>()
                {
                    { "FirstName", "Test User Param" }
                });
        }

        [Test]
        public void MailMessage_Html_WithInlineAttachements()
        {
            var message = BaseMessage();
            var body = "<html><head></head><body><img src=\"cid:test.jpg\"/></body></html>";
            message.Body = body;
            message.IsBodyHtml = true;
            var view = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            Response<DataItem> result;
            using (var bmp = new Bitmap(128, 128))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    using (var s = new MemoryStream())
                    {
                        g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, 128, 128));
                        bmp.Save(s, ImageFormat.Jpeg);
                        view.LinkedResources.Add(new LinkedResource(s, MediaTypeNames.Image.Jpeg) { ContentId = "test.jpg" });
                        message.AlternateViews.Add(view);
                        result = _client.SendMessage(message);
                    }
                }
            }
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void MailMessage_Text_NoAttachements()
        {
            var message = BaseMessage();
            message.Body = "test";
            var result = _client.SendMessage(message);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void MailMessage_Text_NoAttachements_Batch()
        {
            var message = BaseMessage();
            message.To.Add(new MailAddress("v-cefo@microsoft.com"));
            message.To.Add(new MailAddress("cedricf@outlook.com"));

            message.Body = "test";
            var result = _client.SendMessage(message);
            Assert.IsNotNull(result);
            Assert.Greater(2, result.Count);
        }

        [Test]
        public void MailMessage_Text_WithDisplayName()
        {
            var message = new MailMessage();
#if DEBUG
            var testFrom = Environment.GetEnvironmentVariable("MailJetTestFrom", EnvironmentVariableTarget.User);
            var testTo = Environment.GetEnvironmentVariable("MailJetTestTo", EnvironmentVariableTarget.User);
#else
            var testFrom = Environment.GetEnvironmentVariable("MailJetTestFrom");
            var testTo = Environment.GetEnvironmentVariable("MailJetTestTo");
#endif
            message.To.Add(new MailAddress(testTo, "To Test"));
            message.From = new MailAddress(testFrom, "To From");
            message.Subject = "test";
            message.Body = "test";
            var result = _client.SendMessage(message);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void MailMessage_Text_WithAttachements()
        {
            var message = BaseMessage();
            message.Body = "test";
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", "TextFile.txt");
            message.Attachments.Add(new System.Net.Mail.Attachment(path));
            var result = _client.SendMessage(message);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void MailMessage_Html_NoAttachements()
        {
            var message = BaseMessage();
            message.Body = "<b>TEST</b>";
            message.IsBodyHtml = true;
            var result = _client.SendMessage(message);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void MailMessage_Html_Campaign_NoAttachements()
        {
            var message = BaseMailjetMessage();
            message.HtmlPart = "<b>TEST Mailjet without attachement</b> <br /> <a href='http://www.bing.com'>Click me</a>";
            message.MjCampaign = string.Format("TestApi_{0}-{1}", DateTime.Now.Year, DateTime.Now.Month);
            var result = _client.SendMessage(message);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void MailMessage_Html_Campaign_WithAttachements()
        {
            var message = BaseMailjetMessage();
            message.HtmlPart = "<b>TEST Mailjet with attachement</b> <br /> <a href='http://www.bing.com'>Click me</a>";
            message.MjCampaign = string.Format("TestApi_{0}-{1}", DateTime.Now.Year, DateTime.Now.Month);
            message.MjTrackClick = 2; //default value = 1
            message.MjTrackOpen = 2; //default value = 2
            message.MjCustomID = 100;

            //-- Attachement
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", "TextFile.txt");
            //var file = File.Open(path, FileMode.Open);
            var mailjetAttachment = new MailjetAttachment();
            mailjetAttachment.Content = File.ReadAllText(path);
            mailjetAttachment.Filename = "TextFile.txt";
            mailjetAttachment.ContentType = ".txt";
            message.Attachments = new List<MailjetAttachment>();
            message.Attachments.Add(mailjetAttachment);

            var result = _client.SendMessage(message);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        /// <summary>
        /// TODO : Convert Mail message to mailjet in the constructor
        /// </summary>
        public void Mailjet_MailMessage_Html_Convert_NoAttachements()
        {
            var message = BaseMessage();
            message.Body = "<b>TEST</b>";
            message.IsBodyHtml = true;

            var maijetMessage = new MailjetSendMail(message);

            var result = _client.SendMessage(message);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        private string FromAddress
        {
            get
            {
#if DEBUG
                var testFrom = Environment.GetEnvironmentVariable("MailJetTestFrom", EnvironmentVariableTarget.User);
#else
                var testFrom = Environment.GetEnvironmentVariable("MailJetTestFrom");
#endif
                return testFrom;
            }
        }

        private string ToAddress
        {
            get
            {
#if DEBUG
                var testTo = Environment.GetEnvironmentVariable("MailJetTestTo", EnvironmentVariableTarget.User);
#else
                var testTo = Environment.GetEnvironmentVariable("MailJetTestTo");
#endif
                return testTo;
            }
        }

        private MailMessage BaseMessage()
        {
            var message = new MailMessage()
            {
                From = new MailAddress(FromAddress),
                Subject = "test " + DateTime.Now.ToLocalTime()
            };

            message.To.Add(new MailAddress(ToAddress));
            return message;
        }

        private MailjetSendMail BaseMailjetMessage()
        {
            var message = new MailjetSendMail()
            {
                FromEmail = FromAddress,
                Subject = "test " + DateTime.Now.ToLocalTime()
            };

            message.Recipients = new List<MailjetRecipient>();
            message.Recipients.Add(new MailjetRecipient(ToAddress));
            return message;
        }

    }
}

