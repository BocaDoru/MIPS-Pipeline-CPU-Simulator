# MIPS Pipeline CPU Simulator

A fully interactive, visual simulator for a 5-stage MIPS pipeline processor, built in Unity and C#. This tool is designed for educational purposes to visualize data flow, pipeline hazards, and hardware architecture concepts in real-time.

## Key Features

* **5-Stage Pipeline:** Implements the classic Fetch (IF), Decode (ID), Execute (EX), Memory (MEM), and Write Back (WB) stages.
* **Visual Data Path:** Wires connecting components allow for real-time visualization of data flow using a `Signal` and `BitArray` abstraction.
* **Advanced Hazard Management:**
    * **Data Hazards:** Handled via a Forwarding Unit (bypassing the WB stage).
    * **Control Hazards:** Handled via Pipeline Flushing (Branch-in-ID optimization).
    * **Structural/Load-Use Hazards:** Handled via a Hazard Unit that triggers Stalls/Bubbles.
* **Interactive Editors:**
    * **Instruction Memory:** Write Assembly directly (auto-assembled to binary) or edit machine code.
    * **Data Memory:** Hexadecimal view with read/write highlighting.
    * **Register File:** Real-time monitoring of all 32 general-purpose registers.
* **Custom Clock System:** A `Clock.cs` component allows for precise edge-triggered simulation (Rising/Falling edge) independent of the Unity physics loop.

## Architecture

This simulator implements a **Branch-in-ID** architecture to optimize performance.

* **Branch Decision:** Moved to the **ID (Instruction Decode)** stage (standard is MEM). This reduces the flush penalty for taken branches from 3 cycles to 1 cycle.
* **Forwarding Unit:** Monitors ID/EX and EX/MEM registers to forward data to the ID stage (for branch comparisons) and EX stage (for ALU operations).
* **Hazard Unit:** Detects `Load-Use` cases and freezes the PC and IF/ID registers for 2 cycles when necessary.

### Core Components
The simulation relies on a custom `Signal` asset system where components do not reference each other directly but communicate via shared "Wire" assets.
* **Synchronous Components:** `RAM`, `RegisterFile`, `Pipeline Registers` (Update on Clock Edge).
* **Asynchronous Components:** `ALU`, `Control Unit`, `Mux`, `SignalSplitter` (Update continuously).

## Getting Started

### Prerequisites
* **For Users:** Windows OS (to run the executable).
* **For Developers:** Unity Editor **2022.3 LTS** or newer.

### Installation

#### Option A: Running the Executable
1.  Download the latest release from the repository.
2.  Open the `MIPS Pipeline Simulator Executable` folder.
3.  Run `MIPS Pipeline Simulator.exe`.

#### Option B: Opening in Unity
1.  Clone this repository.
2.  Open Unity Hub and click **Add**.
3.  Select the root folder of the cloned repository.
4.  Open the project and press the **Play** button to start the simulation.

## Usage Guide

### Control Interface
* **Start/Pause:** Toggles processor execution.
* **Reload:** Resets the PC to 0 and flushes pipeline registers.
* **Clock Slider:** Adjusts the simulation speed. Slow down to watch bits travel or speed up to finish sorting algorithms.

### Programming the CPU
1.  Open the **Instruction Memory** window.
2.  Type MIPS assembly code into the editor (e.g., `ADD $t0, $t1, $t2`).
3.  The simulator automatically converts this to binary.
4.  Use the **Save/Load** buttons to persist your programs.

### Visual Debugging
* **Wires:** Green lines indicate a bit value of `1`, Black lines indicate `0`.
* **Memory:** Rows in Data Memory turn **Green** when read and **Blue** when written.
* **Registers:** Ports highlight **Cyan/Green** for Read operations and **Blue** for Write operations.

## Supported Instruction Set

The Main Control unit currently supports the following opcodes:

| Instruction | Type | OpCode | Function |
| :--- | :--- | :--- | :--- |
| **ADD, SUB, AND, OR, XOR, SLL, SRL, SRA** | R-Type | 0x00 | Varies |
| **ADDI** | I-Type | 0x08 | - |
| **LW** (Load Word) | I-Type | 0x23 | - |
| **SW** (Store Word) | I-Type | 0x2B | - |
| **BEQ** (Branch Equal) | I-Type | 0x04 | - |
| **BNE** (Branch Not Equal)| I-Type | 0x05 | - |
| **BGEZ** (Branch Greater Or Equal Zero)| I-Type | 0x01 | - |
| **BGTZ** (Branch Greater Than Zero)| I-Type | 0x07 | - |
| **J** (Jump) | J-Type | 0x02 | - |
| **JAL** (Jump And Link) | J-Type | 0x03 | - |
| **JR** (Jump Register) | R-Type | 0x00 | 0x08 |

## Testing & Validation

The simulator includes test files to validate hazard handling:
1.  **Simple Loop:** Verifies basic PC updates and ALU ops.
2.  **Bubble Sort (Optimized):** Runs without NOPs, relying on the Hardware Hazard Unit to stall and Forwarding Unit to bypass data.
3.  **Bubble Sort (Unoptimized):** Uses software NOPs to avoid hazards manually (slower execution).
