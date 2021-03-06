using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Roadkill.Tests.Acceptance.Webdriver
{
	[TestFixture]
	[Category("Acceptance")]
	public class InstallerTests : AcceptanceTestBase
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{	
			TestHelpers.CopyDevConnectionStringsConfig();
		}

		[SetUp]
		public void Setup()
		{
			Driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10)); // for ajax calls
			TestHelpers.SetRoadkillConfigToUnInstalled();
		}

		protected void ClickLanguageLink()
		{
			int english = 0;
			Driver.FindElements(By.CssSelector("ul#language a"))[english].Click();
		}

		[Test]
		public void installation_page_should_display_for_home_page_when_installed_is_false()
		{
			// Arrange


			// Act
			Driver.Navigate().GoToUrl(BaseUrl);

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("div#installer-container")).Count, Is.EqualTo(1));
		}

		[Test]
		public void installation_page_should_display_for_login_page_when_installed_is_false()
		{
			// Arrange
			

			// Act
			Driver.Navigate().GoToUrl(LoginUrl);

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("div#installer-container")).Count, Is.EqualTo(1));
		}

		[Test]
		public void language_selection_should_display_for_first_page()
		{
			// Arrange

			// Act
			Driver.Navigate().GoToUrl(BaseUrl);

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("ul#language li")).Count, Is.GreaterThanOrEqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector("ul#language li"))[0].Text, Is.EqualTo("English"));
		}

		[Test]
		public void step1_web_config_test_button_should_display_success_toast()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);

			// Act
			ClickLanguageLink();
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector("#toast-container")), Is.True);
		}

		[Test]
		public void step1_web_config_test_button_should_display_error_modal_and_no_continue_link_for_readonly_webconfig()
		{
			// Arrange
			string sitePath = TestConstants.WEB_PATH;
			string webConfigPath = Path.Combine(sitePath, "web.config");
			File.SetAttributes(webConfigPath, FileAttributes.ReadOnly);

			// Cascades down
			string roadkillConfigPath = Path.Combine(sitePath, "roadkill.config");
			File.SetAttributes(roadkillConfigPath, FileAttributes.ReadOnly);

			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".bootbox")), Is.True);
		}

		[Test]
		public void step2_connection_test_button_should_display_success_toast_for_good_connectionstring()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
			select.SelectByValue("SqlServer2008");

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(TestConstants.CONNECTION_STRING);
			Driver.FindElement(By.CssSelector("button[id=testdbconnection]")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector("#toast-container")), Is.True);
		}

		[Test]
		public void step2_connection_test_button_should_display_error_modal_for_bad_connectionstring()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
			select.SelectByValue("SqlServer2008");

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"Server=(local);Integrated Security=true;Connect Timeout=5;database=some-database-that-doesnt-exist");
			Driver.FindElement(By.CssSelector("button[id=testdbconnection]")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".bootbox")), Is.True);

		}

		[Test]
		public void step2_missing_site_name_title_should_prevent_continue()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).Clear();
			Driver.FindElement(By.Id("SiteUrl")).SendKeys("not empty");
			Driver.FindElement(By.Id("ConnectionString")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".help-block")), Is.True);
			Assert.That(Driver.FindElement(By.Id("SiteName")).Displayed, Is.True);
		}

		[Test]
		public void step2_missing_site_url_should_prevent_continue()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).SendKeys("not empty");
			Driver.FindElement(By.Id("SiteUrl")).Clear();
			Driver.FindElement(By.Id("ConnectionString")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".help-block")), Is.True);
			Assert.That(Driver.FindElement(By.Id("SiteUrl")).Displayed, Is.True);
		}

		[Test]
		public void step2_missing_connectionstring_should_prevent_contine()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).SendKeys("not empty");
			Driver.FindElement(By.Id("SiteUrl")).SendKeys("not empty");
			Driver.FindElement(By.Id("ConnectionString")).Clear();
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".help-block")), Is.True);
			Assert.That(Driver.FindElement(By.Id("ConnectionString")).Displayed, Is.True);
		}

		[Test]
		public void step3_missing_admin_email_should_prevent_continue()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).SendKeys("not empty");
			Driver.FindElement(By.Id("SiteUrl")).SendKeys("not empty");
			Driver.FindElement(By.Id("ConnectionString")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.Id("AdminEmail")).Clear();
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("not empty");
			Driver.FindElement(By.Id("password2")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".help-block")), Is.True);
			Assert.That(Driver.FindElement(By.Id("AdminEmail")).Displayed, Is.True);
		}

		[Test]
		public void step3_missing_admin_password_should_prevent_continue()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).SendKeys("not empty");
			Driver.FindElement(By.Id("SiteUrl")).SendKeys("not empty");
			Driver.FindElement(By.Id("ConnectionString")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.Id("AdminEmail")).SendKeys("not empty");
			Driver.FindElement(By.Id("AdminPassword")).Clear();
			Driver.FindElement(By.Id("password2")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".help-block")), Is.True);
			Assert.That(Driver.FindElement(By.Id("AdminPassword")).Displayed, Is.True);
		}

		[Test]
		public void step3_not_min_length_admin_password_should_prevent_continue()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).SendKeys("not empty");
			Driver.FindElement(By.Id("SiteUrl")).SendKeys("not empty");
			Driver.FindElement(By.Id("ConnectionString")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.Id("AdminEmail")).SendKeys("not empty");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("1");
			Driver.FindElement(By.Id("password2")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".help-block")), Is.True);
			Assert.That(Driver.FindElement(By.Id("AdminPassword")).Displayed, Is.True);
		}

		[Test]
		public void step3_missing_admin_password2_should_prevent_continue()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).SendKeys("not empty");
			Driver.FindElement(By.Id("SiteUrl")).SendKeys("not empty");
			Driver.FindElement(By.Id("ConnectionString")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.Id("AdminEmail")).SendKeys("not empty");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("not empty");
			Driver.FindElement(By.Id("password2")).Clear();
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".help-block")), Is.True);
			Assert.That(Driver.FindElement(By.Id("password2")).Displayed, Is.True);
		}

		[Test]
		public void step4_test_attachments_folder_button_with_existing_folder_should_display_success_toast()
		{
			// Arrange
			string sitePath = TestConstants.WEB_PATH;
			Guid folderGuid = Guid.NewGuid();
			string attachmentsFolder = Path.Combine(sitePath, "AcceptanceTests", folderGuid.ToString());		
			Directory.CreateDirectory(attachmentsFolder);

			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).SendKeys("not empty");
			Driver.FindElement(By.Id("SiteUrl")).SendKeys("not empty");
			Driver.FindElement(By.Id("ConnectionString")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("not empty");
			Driver.FindElement(By.Id("password2")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.Id("AttachmentsFolder")).Clear();
			Driver.FindElement(By.Id("AttachmentsFolder")).SendKeys("~/AcceptanceTests/" + folderGuid);
			Driver.FindElement(By.CssSelector("button[id=testattachments]")).Click();

			// Assert
			try
			{
				Assert.That(Driver.IsElementDisplayed(By.CssSelector("#toast-container")), Is.True);
			}
			finally
			{
				Directory.Delete(attachmentsFolder, true);
			}
		}

		[Test]
		public void step4_test_attachments_folder_button_with_missing_folder_should_display_failure_modal()
		{
			// Arrange
			Guid folderGuid = Guid.NewGuid();
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).SendKeys("not empty");
			Driver.FindElement(By.Id("SiteUrl")).SendKeys("not empty");
			Driver.FindElement(By.Id("ConnectionString")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("not empty");
			Driver.FindElement(By.Id("password2")).SendKeys("not empty");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.Id("AttachmentsFolder")).Clear();
			Driver.FindElement(By.Id("AttachmentsFolder")).SendKeys("~/" + folderGuid);
			Driver.FindElement(By.CssSelector("button[id=testattachments]")).Click();

			// Assert
			Assert.That(Driver.IsElementDisplayed(By.CssSelector(".bootbox")), Is.True);
		}

		[Test]
		public void navigation_persists_field_values_correctly()
		{
			// Arrange
			string sitePath = TestConstants.WEB_PATH;
			Guid folderGuid = Guid.NewGuid();
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			// Act
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			Driver.FindElement(By.Id("SiteName")).Clear();
			Driver.FindElement(By.Id("SiteName")).SendKeys("Site Name");

			Driver.FindElement(By.Id("SiteUrl")).Clear();
			Driver.FindElement(By.Id("SiteUrl")).SendKeys("Site Url");

			Driver.FindElement(By.Id("ConnectionString")).Clear();
			Driver.FindElement(By.Id("ConnectionString")).SendKeys("Connection String");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
			select.SelectByValue("MySQL");

			Driver.FindElement(By.CssSelector("div.continue button")).Click();
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			Driver.FindElement(By.CssSelector("div.previous a")).Click();
			Driver.FindElement(By.CssSelector("div.previous a")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.Id("SiteName")).GetAttribute("value"), Is.EqualTo("Site Name"));
			Assert.That(Driver.FindElement(By.Id("SiteUrl")).GetAttribute("value"), Is.EqualTo("Site Url"));
			Assert.That(Driver.FindElement(By.Id("ConnectionString")).GetAttribute("value"), Is.EqualTo("Connection String"));

			select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
			Assert.That(select.SelectedOption.GetAttribute("value"), Is.EqualTo("MySQL"));
		}

		[TestFixture]
		[Category("Acceptance")]
		public class OtherDatabases : AcceptanceTestBase
		{
			protected void ClickLanguageLink()
			{
				int english = 0;
				Driver.FindElements(By.CssSelector("ul#language a"))[english].Click();
			}

			[Test]
			[Explicit("Requires MySQL 5 installed on the machine the acceptance tests are running first.")]
			public void MySQL_All_Steps_With_Minimum_Required()
			{
				// Arrange
				Driver.Navigate().GoToUrl(BaseUrl);
				ClickLanguageLink();

				//
				// ***Act***
				//

				// step 1
				Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
				Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

				// step 2
				Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
				SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
				select.SelectByValue("MySQL");

				Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"server=localhost;database=roadkill;uid=root;pwd=Passw0rd;");
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 3
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 3b
				Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
				Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
				Driver.FindElement(By.Id("password2")).SendKeys("password");
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 4
				Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step5
				Assert.That(Driver.FindElement(By.CssSelector(".alert strong")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
				Driver.FindElement(By.CssSelector(".continue a")).Click();

				// login, create a page
				LoginAsAdmin();
				CreatePageWithTitleAndTags("Homepage", "homepage");

				//
				// ***Assert***
				//
				Driver.Navigate().GoToUrl(BaseUrl);
				Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
				Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
			}

			[Test]
			[Explicit("Requires Postgres 9 server installed on the machine the acceptance tests are running first.")]
			public void Postgres_All_Steps_With_Minimum_Required()
			{
				// Arrange
				Driver.Navigate().GoToUrl(BaseUrl);
				ClickLanguageLink();

				//
				// ***Act***
				//

				// step 1
				Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
				Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

				// step 2
				Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
				SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
				select.SelectByValue("Postgres");

				Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"server=localhost;database=roadkill;uid=postgres;pwd=Passw0rd;");
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 3
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 3b
				Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
				Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
				Driver.FindElement(By.Id("password2")).SendKeys("password");
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 4
				Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step5
				Assert.That(Driver.FindElement(By.CssSelector(".alert strong")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
				Driver.FindElement(By.CssSelector(".continue a")).Click();

				// login, create a page
				LoginAsAdmin();
				CreatePageWithTitleAndTags("Homepage", "homepage");

				//
				// ***Assert***
				//
				Driver.Navigate().GoToUrl(BaseUrl);
				Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
				Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
			}

			[Test]
			[Explicit("Requires SQL Server Express 2012 (but it uses the Lightspeed SQL Server 2005 driver) installed on the machine the acceptance tests are running first, using LocalDB.")]
			public void SQLServer2005Driver_All_Steps_With_Minimum_Required()
			{
				// Arrange
				Driver.Navigate().GoToUrl(BaseUrl);
				ClickLanguageLink();

				//
				// ***Act***
				//

				// step 1
				Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
				Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

				// step 2
				Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
				SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
				select.SelectByValue("SqlServer2008");

				Driver.FindElement(By.Id("ConnectionString")).SendKeys(TestConstants.CONNECTION_STRING);
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 3
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 3b
				Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
				Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
				Driver.FindElement(By.Id("password2")).SendKeys("password");
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 4
				Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step5
				Assert.That(Driver.FindElement(By.CssSelector(".alert strong")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
				Driver.FindElement(By.CssSelector(".continue a")).Click();

				// login, create a page
				LoginAsAdmin();
				CreatePageWithTitleAndTags("Homepage", "homepage");

				//
				// ***Assert***
				//
				Driver.Navigate().GoToUrl(BaseUrl);
				Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
				Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
			}

			[Test]
			[Explicit(@"This is really a helper test, it installs onto .\SQLEXPRESS, database 'roadkill' using integrated security")]
			public void SQLServerExpress_All_Steps_With_Minimum_Required()
			{
				// Arrange
				Driver.Navigate().GoToUrl(BaseUrl);
				ClickLanguageLink();

				//
				// ***Act***
				//

				// step 1
				Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
				Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

				// step 2
				Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
				SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
				select.SelectByValue("SqlServer2008");

				Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"Server=.\SQLEXPRESS;Integrated Security=true;database=roadkill");
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 3
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 3b
				Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
				Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
				Driver.FindElement(By.Id("password2")).SendKeys("password");
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step 4
				Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
				Driver.FindElement(By.CssSelector("div.continue button")).Click();

				// step5
				Assert.That(Driver.FindElement(By.CssSelector(".alert strong")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
				Driver.FindElement(By.CssSelector(".continue a")).Click();

				// login, create a page
				LoginAsAdmin();
				CreatePageWithTitleAndTags("Homepage", "homepage");

				//
				// ***Assert***
				//
				Driver.Navigate().GoToUrl(BaseUrl);
				Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
				Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
			}
		}
	}
}
