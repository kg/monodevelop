//
// DocumentManagerTests.cs
//
// Author:
//       Lluis Sanchez <llsan@microsoft.com>
//
// Copyright (c) 2019 Microsoft
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
using MonoDevelop.Core;
using UnitTests;
using NUnit.Framework;
using System.Threading.Tasks;
using MonoDevelop.Ide.Gui.Documents;
using IdeUnitTests;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Ide.Gui.Shell;
using System.IO;

namespace MonoDevelop.Ide.Gui
{
	public class DocumentManagerTests: TestBase
	{
		BasicServiceProvider serviceProvider;
		DocumentManager documentManager;
		DocumentManagerEventTracker eventTracker;
		DocumentControllerService documentControllerService;
		MockShell shell;

		[SetUp]
		public async Task Setup ()
		{
			serviceProvider = ServiceHelper.SetupMockShell ();
			documentManager = await serviceProvider.GetService<DocumentManager> ();
			shell = await serviceProvider.GetService<IShell> () as MockShell;
			eventTracker = new DocumentManagerEventTracker (documentManager);
			documentControllerService = await serviceProvider.GetService<DocumentControllerService> ();
		}

		[TearDown]
		public async Task TestTearDown ()
		{
			await serviceProvider.Dispose ();
		}

		[Test]
		public async Task OpenCloseController ()
		{
			Assert.AreEqual (0, documentManager.Documents.Count);

			var doc = await documentManager.OpenDocument (new TestController ());

			Assert.AreEqual (1, documentManager.Documents.Count);

			Assert.AreEqual (1, eventTracker.DocumentOpenedEvents.Count);
			Assert.AreSame (doc, eventTracker.DocumentOpenedEvents [0].Document);

			Assert.AreEqual (1, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.AreSame (doc, eventTracker.ActiveDocumentChangedEvents [0].Document);

			Assert.AreEqual (1, shell.Windows.Count);
			var window = shell.Windows [0];
			Assert.AreSame (doc, window.Document);

			eventTracker.Reset ();

			await doc.Close (true);

			Assert.AreEqual (0, shell.Windows.Count);

			Assert.AreEqual (0, eventTracker.DocumentOpenedEvents.Count);

			Assert.AreEqual (1, eventTracker.DocumentClosedEvents.Count);
			Assert.AreSame (doc, eventTracker.DocumentClosedEvents [0].Document);

			Assert.AreEqual (1, eventTracker.DocumentClosingEvents.Count);
			Assert.AreSame (doc, eventTracker.DocumentClosingEvents [0].Document);

			Assert.AreEqual (1, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.IsNull (eventTracker.ActiveDocumentChangedEvents [0].Document);
			Assert.IsNull (documentManager.ActiveDocument);
		}

		[Test]
		public async Task ActiveDocument ()
		{
			// Open one document

			Assert.AreEqual (0, documentManager.Documents.Count);

			var doc1 = await documentManager.OpenDocument (new TestController ());

			Assert.AreEqual (1, documentManager.Documents.Count);
			Assert.AreSame (doc1, documentManager.Documents[0]);

			Assert.AreEqual (1, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.AreSame (doc1, eventTracker.ActiveDocumentChangedEvents [0].Document);

			// Open second

			eventTracker.Reset ();
			var doc2 = await documentManager.OpenDocument (new TestController (), false);

			Assert.AreEqual (2, documentManager.Documents.Count);
			Assert.AreSame (doc1, documentManager.Documents [0]);
			Assert.AreSame (doc2, documentManager.Documents [1]);
			Assert.AreEqual (0, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.AreSame (doc1, documentManager.ActiveDocument);

			// Open third

			eventTracker.Reset ();
			var doc3 = await documentManager.OpenDocument (new TestController ());

			Assert.AreEqual (3, documentManager.Documents.Count);
			Assert.AreSame (doc1, documentManager.Documents [0]);
			Assert.AreSame (doc2, documentManager.Documents [1]);
			Assert.AreSame (doc3, documentManager.Documents [2]);
			Assert.AreEqual (1, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.AreSame (doc3, eventTracker.ActiveDocumentChangedEvents [0].Document);
			Assert.AreSame (doc3, documentManager.ActiveDocument);

			// Select third again

			eventTracker.Reset ();
			doc3.Select ();

			Assert.AreEqual (0, eventTracker.ActiveDocumentChangedEvents.Count);

			// Select second

			eventTracker.Reset ();
			doc2.Select ();
			Assert.AreEqual (1, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.AreSame (doc2, eventTracker.ActiveDocumentChangedEvents [0].Document);
			Assert.AreSame (doc2, documentManager.ActiveDocument);

			// Select first through underlying window

			eventTracker.Reset ();
			shell.Windows [0].SelectWindow ();
			Assert.AreEqual (1, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.AreSame (doc1, eventTracker.ActiveDocumentChangedEvents [0].Document);
			Assert.AreSame (doc1, documentManager.ActiveDocument);

			// Select again is no-op

			eventTracker.Reset ();
			shell.Windows [0].SelectWindow ();
			Assert.AreEqual (0, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.AreSame (doc1, documentManager.ActiveDocument);

			// Close unselects

			eventTracker.Reset ();
			await doc1.Close (true);
			Assert.AreEqual (1, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.AreSame (doc2, eventTracker.ActiveDocumentChangedEvents [0].Document);
			Assert.AreSame (doc2, documentManager.ActiveDocument);

			eventTracker.Reset ();
			await doc2.Close (true);
			Assert.AreEqual (1, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.AreSame (doc3, eventTracker.ActiveDocumentChangedEvents [0].Document);
			Assert.AreSame (doc3, documentManager.ActiveDocument);

			eventTracker.Reset ();
			await doc3.Close (true);
			Assert.AreEqual (1, eventTracker.ActiveDocumentChangedEvents.Count);
			Assert.IsNull (documentManager.ActiveDocument);
		}

		[Test]
		public async Task PreventDocumentClose ()
		{
			var doc = await documentManager.OpenDocument (new TestController ());

			documentManager.DocumentClosing += async (o, e) => {
				await Task.Delay (100); e.Cancel = true;
			};

			eventTracker.Reset ();

			bool result = await doc.Close (true);
			Assert.IsFalse (result);

			Assert.AreEqual (1, shell.Windows.Count);
			Assert.AreEqual (1, documentManager.Documents.Count);
			Assert.AreEqual (0, eventTracker.DocumentClosedEvents.Count);
			Assert.AreEqual (0, eventTracker.ActiveDocumentChangedEvents.Count);
		}

		[Test]
		public async Task GetDocument ()
		{
			documentControllerService.RegisterFactory (new TestFileControllerFactory ());

			FilePath tempDir = FileService.CreateTempDirectory ();
			try {
				var file1 = tempDir.Combine ("aa", "bb", "foo1.test");
				var file2 = tempDir.Combine ("aa", "bb", "cc", "foo1.test");
				var file3 = tempDir.Combine ("aa", "bb", "cc", "..", "foo3.test");

				Directory.CreateDirectory (file1.ParentDirectory);
				Directory.CreateDirectory (file2.ParentDirectory);
				Directory.CreateDirectory (file3.ParentDirectory);

				File.WriteAllText (file1, "");
				File.WriteAllText (file2, "");
				File.WriteAllText (file3, "");

				var doc1 = await documentManager.OpenDocument (new FileOpenInformation (file1));
				var doc2 = await documentManager.OpenDocument (new FileOpenInformation (file2));
				var doc3 = await documentManager.OpenDocument (new FileOpenInformation (file3));

				Assert.AreEqual (3, documentManager.Documents.Count);
				Assert.AreSame (doc1, documentManager.Documents [0]);
				Assert.AreSame (doc2, documentManager.Documents [1]);
				Assert.AreSame (doc3, documentManager.Documents [2]);

				var sel = documentManager.GetDocument ("foo1.test");
				Assert.IsNull (sel);

				sel = documentManager.GetDocument ("a.test");
				Assert.IsNull (sel);

				sel = documentManager.GetDocument (file2);
				Assert.AreSame (doc2, sel);

				sel = documentManager.GetDocument (tempDir.Combine ("aa","bb","cc","dd","..","foo1.test"));
				Assert.AreSame (doc2, sel);

				sel = documentManager.GetDocument (tempDir.Combine ("aa", "bb", "foo3.test"));
				Assert.AreSame (doc3, sel);
			} finally {
				Directory.Delete (tempDir, true);
			}
		}

		// Test disposing view hierarchy disposes controllers
	}

	class TestController: DocumentController
	{
	}

	class TestFileController : FileDocumentController
	{
	}

	class TestFileControllerFactory : FileDocumentControllerFactory
	{
		public override Task<DocumentController> CreateController (FileDescriptor modelDescriptor, DocumentControllerDescription controllerDescription)
		{
			return Task.FromResult<DocumentController> (new TestFileController ());
		}

		public override IEnumerable<DocumentControllerDescription> GetSupportedControllers (FileDescriptor modelDescriptor)
		{
			if (modelDescriptor.FilePath.Extension == ".test") {
				yield return new DocumentControllerDescription ("Test Source View", true, DocumentControllerRole.Source);
				yield return new DocumentControllerDescription ("Test Design View", false, DocumentControllerRole.VisualDesign);
			}
		}
	}

	class DocumentManagerEventTracker
	{
		private readonly DocumentManager documentManager;

		public List<DocumentEventArgs> DocumentOpenedEvents = new List<DocumentEventArgs> ();
		public List<DocumentEventArgs> DocumentClosedEvents = new List<DocumentEventArgs> ();
		public List<DocumentCloseEventArgs> DocumentClosingEvents = new List<DocumentCloseEventArgs> ();
		public List<DocumentEventArgs> ActiveDocumentChangedEvents = new List<DocumentEventArgs> ();

		public DocumentManagerEventTracker (DocumentManager documentManager)
		{
			this.documentManager = documentManager;

			documentManager.DocumentOpened += (sender, e) => DocumentOpenedEvents.Add (e);
			documentManager.DocumentClosed += (sender, e) => DocumentClosedEvents.Add (e);
			documentManager.DocumentClosing += (sender, e) => { DocumentClosingEvents.Add (e); return Task.CompletedTask; };
			documentManager.ActiveDocumentChanged += (sender, e) => ActiveDocumentChangedEvents.Add (e);
		}

		public void Reset ()
		{
			DocumentOpenedEvents.Clear ();
			DocumentClosedEvents.Clear ();
			DocumentClosingEvents.Clear ();
			ActiveDocumentChangedEvents.Clear ();
		}
	}
}