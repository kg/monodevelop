﻿using System;
using System.Threading.Tasks;
using Xwt;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Components;
using System.Threading;

namespace MonoDevelop.ConnectedServices.DebugService
{
	sealed class TestDebugServiceProvider : IConnectedServiceProvider
	{
		public TestDebugServiceProvider ()
		{
		}

		public IConnectedService GetConnectedService (DotNetProject project)
		{
			#if DEBUG
			if (project.LanguageName == "C#") { 
				return new TestDebugService (project);
			}
			#endif
			return null;
		}
	}

	#if DEBUG
	sealed class TestDebugService : ConnectedService
	{
		public TestDebugService (DotNetProject project) : base(project)
		{
			this.Id = "MonoDevelop.ConnectedServices.DebugService.TestDebugService";
			this.DisplayName = "Test Service";
			this.Description = "This is a simple service example to show how you might construct your own service implementation.";

			this.Dependencies = new IConnectedServiceDependency [] {
				new PackageDependency (this, "Newtonsoft.Json", "Newtonsoft.Json", "6.0.8"),
			};

			this.Sections = new IConfigurationSection [] {
				new TestDebugConfigurationSection(this),
			};
		}

		protected override void OnStoreAddedState(ConnectedServiceState state)
		{
			base.OnStoreAddedState (state);
			state.ProviderId = "MonoDevelop.TestDebugService";
			state.GettingStartedDocument = "https://www.google.com/webhp?q=how+do+i+get+started";
			state.Version = "1.1";
		}
	}

	sealed class TestDebugConfigurationSection : ConfigurationSection
	{
		bool added;
		
		public override bool IsAdded {
			get { return added; }
		}
		
		public TestDebugConfigurationSection (IConnectedService service) : base (service, "Configure a setting")
		{
		}

		public override Control GetSectionWidget ()
		{
			return new TestConfigurationWidget ();
		}

		protected override Task<bool> OnAddToProject (CancellationToken token)
		{
			added = true;
			return Task.FromResult (true);
		}
	}

	sealed class TestConfigurationWidget : AbstractXwtControl
	{
		readonly Widget content;
		
		public TestConfigurationWidget ()
		{
			var vbox = new VBox ();

			var label = new Label { Text = "this configures a setting" };
			vbox.PackStart (label);

			content = vbox;
		}

		public override Widget Widget {
			get {
				return content;
			}
		}
	}
#endif
}
