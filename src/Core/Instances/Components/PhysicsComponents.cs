using ColonThreePet.Core.Math;
using static System.Math;

namespace ColonThreePet.Core.Instances;

public sealed class PhysicsComponent : Instance {
	// property fields
	private bool isEnabled = true, isActive = false, canCollide = true, isAnchored = false, affectedByGravity = true;
	private double bounciness = 0d, friction = 0.5, standingOverlap;
	private PhysicsComponent? standingOnFrontBuffer, standingOnBackBuffer;
	private double vx, vy;

	public bool IsEnabled {
		get => this.isEnabled;
		set {
			this.setPropertyAndNotify(ref this.isEnabled, value);
			this.updateIsActive();
		}
	}

	public bool IsActive {
		get => this.isActive;
		private set => this.setPropertyAndNotify(ref this.isActive, value);
	}

	public bool CanCollide {
		get => this.canCollide;
		set => this.setPropertyAndNotify(ref this.canCollide, value);
	}

	public bool IsAnchored {
		get => this.isAnchored;
		set => this.setPropertyAndNotify(ref this.isAnchored, value);
	}

	public bool IsAffectedByGravity {
		get => this.affectedByGravity;
		set => this.setPropertyAndNotify(ref this.affectedByGravity, value);
	}

	public double Bounciness {
		get => this.bounciness;
		set => this.setPropertyAndNotify(ref this.bounciness, value);
	}

	public double Friction {
		get => this.friction;
		set => this.setPropertyAndNotify(ref this.friction, value);
	}

	public PhysicsComponent? StandingOn {
		get => this.standingOnFrontBuffer;
		set => this.setPropertyAndNotify(ref this.standingOnFrontBuffer, value);
	}

	public Vector2d Velocity {
		get => new Vector2d(this.vx, this.vy);
		set {
			if (this.vx == value.X && this.vy == value.Y) return;
			this.vx = value.X;
			this.vy = value.Y;
			this.notifyPropertyChange();
		}
	}

	private void updateIsActive() {
		// checks that it's enabled, the parent is a WorldInstance, and that it's in the Workspace
		bool toSet = this.isEnabled && this.Parent is WorldInstance && this.IsAncestorOf(Engine.Workspace);
		if (!toSet) {
			this.IsActive = false;
			return;
		}

		// checks that it's the only PhysicsComponent inside it's parent
		this.IsActive = this.Parent.GetChildrenOfType<PhysicsComponent>().Count == 1;
	}

	internal void step(double h) {
		var parent = (WorldInstance)this.Parent;

		double oldVx = this.vx, oldVy = this.vy;
		if (this.affectedByGravity) this.vy += Engine.Workspace.Gravity * h;
		double damp = Exp(-Engine.Workspace.AirDrag * h);
		this.vx *= damp;
		this.vy *= damp;

		var position = new Vector2d(parent.Position.X + this.vx * h, parent.Position.Y);
		if (this.canCollide) this.resolve(ref position, horizontal: true);

		position = new Vector2d(position.X, position.Y + this.vy * h);
		if (this.canCollide) this.resolve(ref position, horizontal: false);

		parent.Position = position;
		if (this.vx != oldVx || this.vy != oldVy)
			this.notifyPropertyChange(nameof(this.Velocity));

		this.StandingOn = this.standingOnBackBuffer;
	}

	private void resolve(ref Vector2d position, bool horizontal) {
		var a = (WorldInstance)this.Parent;
		foreach (var other in Engine.Workspace.PhysicsComponentsIndex) {
			if (other == this || !other.isActive || !other.canCollide) continue;
			var b = (WorldInstance)other.Parent;
			double ax = position.X, ay = position.Y, aw = a.Size.X, ah = a.Size.Y;
			double bx = b.Position.X, by = b.Position.Y, bw = b.Size.X, bh = b.Size.Y;

			double overlapX = Min(ax + aw, bx + bw) - Max(ax, bx);
			double overlapY = Min(ay + ah, by + bh) - Max(ay, by);
			if (overlapX <= 0 || overlapY <= 0) continue; // touching is not colliding

			if (horizontal) {
				// normal points from the other object towards us
				double n = (ax + aw / 2) < (bx + bw / 2) ? -1 : 1;
				position = new Vector2d(ax + n * overlapX, ay);
				this.respond(other, n, horizontal, supportWidth: overlapY);
			} else {
				double n = (ay + ah / 2) < (by + bh / 2) ? -1 : 1; // -1 = we are above
				position = new Vector2d(ax, ay + n * overlapY);
				this.respond(other, n, horizontal, supportWidth: overlapX);
			}
		}
	}

	private void respond(PhysicsComponent other, double n, bool horizontal, double supportWidth) {
		// Velocity components along (vn) and across (vt) the collision normal.
		double vn = horizontal ? this.vx : this.vy;
		double vt = horizontal ? this.vy : this.vx;
		double ovn = 0, ovt = 0;
		if (!other.isAnchored) {
			ovn = horizontal ? other.vx : other.vy;
			ovt = horizontal ? other.vy : other.vx;
		}

		// Closing speed (negative = approaching along the normal).
		double rel = (vn - ovn) * n;
		if (rel >= 0) return; // already separating

		// Inverse masses: all objects have equal mass, anchored ones are immovable.
		double invA = 1.0;
		double invB = other.isAnchored ? 0.0 : 1.0;

		// Bounce
		double e = Max(Bounciness, other.Bounciness);
		if (-rel < Engine.Workspace.BounceThreshold) e = 0;

		double j = -(1 + e) * rel / (invA + invB); // normal impulse magnitude
		vn += j * invA * n;
		ovn -= j * invB * n;

		// Friction (Coulomb): tangential impulse limited to mu * normal impulse
		double mu = Sqrt(Max(0, Friction) * Max(0, other.Friction));
		double relT = vt - ovt;
		if (relT != 0) {
			double jt = Min(Abs(relT) / (invA + invB), mu * j);
			double dir = Sign(relT);
			vt -= dir * jt * invA;
			ovt += dir * jt * invB;
		}

		// Write back
		if (horizontal) {
			this.vx = vn;
			this.vy = vt;
		} else {
			this.vy = vn;
			this.vx = vt;
		}

		if (!other.isAnchored) {
			other.Velocity = horizontal ? new Vector2d(ovn, ovt) : new Vector2d(ovt, ovn);
		}

		if (!horizontal && n < 0 && supportWidth > this.standingOverlap) {
			this.standingOnBackBuffer = other;
			this.standingOverlap = supportWidth;
		}
	}

	protected override void onParentChange(Instance? oldParent) {
		// notify the old parent to update its PhysicsComponent
		if (oldParent is WorldInstance) {
			var siblings = oldParent.GetChildrenOfType<PhysicsComponent>();
			if (siblings.Count == 1) {
				siblings[0].updateIsActive();
			}
		}

		// check how many PhysicsComponents there are and disable if more than one
		if (this.Parent is WorldInstance) {
			var siblings = this.Parent.GetChildrenOfType<PhysicsComponent>();
			if (siblings.Count > 1) {
				foreach (var sibling in siblings) {
					// just set it to false because there's more than one PhysicsComponent
					sibling.IsActive = false;
				}
			}
		}
	}

	public PhysicsComponent() {
		this.AncestryChanged += (sender, args) => this.updateIsActive();
	}

	internal static void stepWorld(double dt, double maxSubstep = 1d / 120d) {
		if (dt <= 0) return;
		var list = Engine.Workspace.PhysicsComponentsIndex;
		
		for (int i = 0; i < list.Count; i++) {
			list[i].standingOnBackBuffer = null;
			list[i].standingOverlap = 0d;
		}

		int steps = Max(1, (int)Ceiling(dt / maxSubstep));
		double h = dt / steps;

		for (int s = 0; s < steps; s++) {
			// could probably error if you insert mid read
			// will fix later
			foreach (var pc in list) {
				if (pc.isActive && !pc.isAnchored)
					pc.step(h);
			}
		}
	}
}