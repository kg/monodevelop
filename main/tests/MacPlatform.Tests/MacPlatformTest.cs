﻿// 
// MacPlatformTest.cs
//  
// Author:
//       Alan McGovenrn <alan@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc.
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
using NUnit.Framework;
using MonoDevelop.MacIntegration;
using MonoDevelop.MacInterop;
using MonoDevelop.Ide;
using UnitTests;
using System.Threading.Tasks;

namespace MacPlatform.Tests
{
	[TestFixture]
	public class MacPlatformTest : IdeTestBase
	{
		[Test]
		public void GetMimeType_text ()
		{
			// Verify no exception is thrown
			IdeApp.DesktopService.GetMimeTypeForUri ("test.txt");
		}

		[Test]
		public void GetMimeType_NoExtension ()
		{
			// Verify no exception is thrown
			IdeApp.DesktopService.GetMimeTypeForUri ("test");
		}

		[Test]
		public void GetMimeType_Null ()
		{
			// Verify no exception is thrown
			IdeApp.DesktopService.GetMimeTypeForUri (null);
		}

		[Test]
		public void MacHasProperMonitor ()
		{
			Assert.That (IdeApp.DesktopService.MemoryMonitor, Is.TypeOf<MacPlatformService.MacMemoryMonitor> ());
		}

		[Test, Timeout(20000)]
		public async Task TestMacMemoryMonitorLifetime ()
		{
			var tcs = new TaskCompletionSource<bool> ();

			using (var macMonitor = new MacPlatformService.MacMemoryMonitor ()) {
				// Cancellation is async.
				macMonitor.DispatchSource.SetCancelHandler (() => tcs.SetResult (true));
			}

			Assert.AreEqual (true, await tcs.Task, "Expected cancel handler to be called");
		}
	}
}

