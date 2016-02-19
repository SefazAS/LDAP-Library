using System;
using LDAPLibrary.Enums;
using LDAPLibrary.Factories;
using LDAPLibrary.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LDAP_Library_UnitTest
{
    [TestClass]
    public class LoggerFactoryUnitTests
    {
        [TestMethod, TestCategory("LoggerFactoryFileLogger")]
        public void LoggerFactoryFileLogger()
        {
            var logger = LoggerFactory.GetLogger(LoggerType.File, AppDomain.CurrentDomain.BaseDirectory);

            Assert.IsInstanceOfType(logger, typeof (FileLogger));
        }

        [TestMethod, TestCategory("LoggerFactoryFakeLogger")]
        public void LoggerFactoryFakeLogger()
        {
            var logger = LoggerFactory.GetLogger(LoggerType.None, null);

            Assert.IsInstanceOfType(logger, typeof (FakeLogger));
        }

        [TestMethod, TestCategory("LoggerFactoryEventViewerLogger")]
        public void LoggerFactoryEventViewer()
        {
            var logger = LoggerFactory.GetLogger(LoggerType.EventViewer, null);

            Assert.IsInstanceOfType(logger, typeof (EventViewerLogger));
        }
    }
}