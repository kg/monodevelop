//
// DocumentReloadTests.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2018 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Core.Text;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Documents;
using NUnit.Framework;

namespace MonoDevelop.Ide.Editor
{
	[TestFixture]
	public class DocumentReloadTests : IdeTestBase
	{
		[Test]
		public async Task Refactor_RenameClassNameAndFileName_ShouldNotPromptToReload ()
		{
			FilePath directory = UnitTests.Util.CreateTmpDir ("FileRenameShouldNotPromptToReload");
			FilePath fileName = directory.Combine ("test.cs");
			File.WriteAllText (fileName, "class Test {}");

			var content = new TestViewContentWithDocumentReloadPresenter ();
			content.FilePath = fileName;

			using (var testCase = await TextEditorExtensionTestCase.Create (content, null, false)) {
				var doc = testCase.Document;

				bool reloadWarningDisplayed = false;
				content.OnShowFileChangeWarning = multiple => {
					reloadWarningDisplayed = true;
				};
				doc.Editor.Text = "class rename {}";
				doc.IsDirty = true; // Refactor leaves file unsaved in text editor.
				FilePath newFileName = fileName.ChangeName ("renamed");
				FileService.RenameFile (fileName, newFileName);
				// Simulate DefaultWorkbench which updates the view content name when the FileService
				// fires the rename event.
				content.FilePath = newFileName;
				FileService.NotifyFileChanged (newFileName);

				Assert.IsFalse (reloadWarningDisplayed);
			}
		}

		class TestViewContentWithDocumentReloadPresenter : TestViewContent, IDocumentReloadPresenter
		{
			public Document Document { get; set; }

			public void RemoveMessageBar ()
			{
			}

			public Action<bool> OnShowFileChangeWarning = multiple => { };

			public void ShowFileChangedWarning (bool multiple)
			{
				OnShowFileChangeWarning (multiple);
			}

			protected override async Task OnInitialize (ModelDescriptor modelDescriptor, Properties status)
			{
				var file = (FileDescriptor)modelDescriptor;
				var fileName = file.FilePath;
				var content = await TextFileUtility.ReadAllTextAsync (fileName);
				Document.Editor.Text = (await TextFileUtility.ReadAllTextAsync (fileName)).Text;
				DocumentTitle = fileName;
			}
		}
	}
}
