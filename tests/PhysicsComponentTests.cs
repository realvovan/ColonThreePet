using ColonThreePet.Core;
using ColonThreePet.Core.Instances;

namespace ColonThreePet.Tests;

public class PhysicsComponentTests {
	[StaTheory]
	[InlineData(false)]
	[InlineData(true)]
	public void SetIsEnable_WhenActive_ShouldUpdateIsActive(bool enabled) {
		var part = new WorldInstance {
			Parent = Engine.Workspace
		};
		var pc = new PhysicsComponent {
			Parent = part
		};

		pc.IsEnabled = enabled;

		Assert.Equal(enabled, pc.IsActive);
		Assert.Equal(enabled, pc.IsEnabled);
	}

	[StaTheory]
	[InlineData(false)]
	[InlineData(true)]
	public void SetIsEnabled_WhenNotInWorldInstance_ShouldSetActiveFalse(bool enabled) {
		// using rect render component as its not a world instance
		var parent = new RectangleRenderComponent { Parent = Engine.Workspace };
		var pc = new PhysicsComponent {
			Parent = parent
		};

		pc.IsEnabled = enabled;

		Assert.Equal(enabled, pc.IsEnabled);
		Assert.False(pc.IsActive);
	}

	[StaTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void SetIsEnable_WhenNotInWorkspace_ShouldSetActiveFalse(bool enabled) {
		var part = new WorldInstance {
			Parent = Engine.Workspace
		};
		var pc = new PhysicsComponent {
			Parent = part
		};
		bool initial = pc.IsActive;

		part.Parent = null!;
		pc.IsEnabled = enabled;

		Assert.True(initial);
		Assert.Equal(enabled, pc.IsEnabled);
		Assert.False(pc.IsActive);
	}
}