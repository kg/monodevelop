//
// FileDescriptor.cs
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
using MonoDevelop.Projects;
using System.IO;
namespace MonoDevelop.Ide.Gui.Documents
{
	/// <summary>
	/// Describes a file
	/// </summary>
	public class FileDescriptor : ModelDescriptor
	{
		public FileDescriptor (FilePath filePath, string mimeType, WorkspaceObject owner)
		{
			FilePath = filePath;
			MimeType = mimeType;
			Owner = owner;
		}

		public FileDescriptor (string fileName, string mimeType, Stream content, WorkspaceObject owner)
		{
			FilePath = fileName;
			MimeType = mimeType;
			Owner = owner;
			Content = content;
		}

		public FilePath FilePath { get; }
		public string MimeType { get; }
		public WorkspaceObject Owner { get; }
		public Stream Content { get; }
	}
}
