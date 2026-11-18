using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace ColonThreePet.Core.RunServiceUtils;

sealed class HighResTimer : IDisposable {
	const uint CREATE_WAITABLE_TIMER_HIGH_RESOLUTIONS = 0x0000_0002;
	const uint TIMER_ALL_ACCESS = 0x1F_0003;
	const uint INFINITE = 0xFFFF_FFFF;

	private readonly SafeWaitHandle handle;

	public bool IsAvailable => !this.handle.IsInvalid;

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	static extern SafeWaitHandle CreateWaitableTimerExW(
		IntPtr attributes, string? name, uint flags, uint desiredAccess);

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern bool SetWaitableTimer(
		SafeWaitHandle timer, in long dueTime, int period,
		IntPtr completionRoutine, IntPtr arg, bool resume);

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern uint WaitForSingleObject(SafeWaitHandle handle, uint milliseconds);

	public HighResTimer() {
		this.handle = CreateWaitableTimerExW(
			IntPtr.Zero, null, CREATE_WAITABLE_TIMER_HIGH_RESOLUTIONS, TIMER_ALL_ACCESS);
	}

	public void Wait(double seconds) {
		if (seconds <= 0) return;

		long due = -(long)(seconds * 10_000_000);
		if (SetWaitableTimer(this.handle, in due, 0, IntPtr.Zero, IntPtr.Zero, false))
			WaitForSingleObject(this.handle, INFINITE);

	}

	public void Dispose() => this.handle.Dispose();
}