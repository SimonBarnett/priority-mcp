# PRI-02 no-token dialog

When the fuel reading is 0 percent, `FuelCheck` shows the no-token dialog and throws `stopped: no tokens`. It does not start the agent and it does not ask to continue.

`NoTokenGate.Apply(0, ...)` returns 2 after the dialog hook. A positive remaining value returns 0.

This seat has no .NET 8 SDK. The gate and driver were compiled with Framework `csc`. A live MessageBox was not shown.
