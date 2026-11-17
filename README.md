# unity-vehicle-follow

Vehicle-follow behavior for Unity: a pure C# state machine and steering core, a WheelCollider adapter, a sample scene setup and EditMode tests.

> **Development happens on the [`dev`](../../tree/dev) branch.**
> The full package (UPM layout, docs, sample and tests) lives there. This branch only holds this note and the license.

To try it, add the `dev` branch to a Unity project's `Packages/manifest.json`:

```json
"com.schaeffergui.vehicle-follow": "https://github.com/schaeffergui/unity-vehicle-follow.git#dev"
```

Status: early release (0.1.0). It compiles in Unity 6000.2 and the 15 EditMode tests pass. See the README on `dev` for what has and has not been verified.

Licensed under the MIT License.
