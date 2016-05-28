﻿#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //
#endregion
    
//#define SCHEDULER_TRACE
//#define SCHEDULER_HANDLER_TRACE
//#define SCHEDULER_HANDLER_MIN_TRACE
//#define SCHEDULER_UPDATE_LIST_TRACE

using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.Processes
{
    public static class Scheduler
    {
        public enum Priority : int
        {
            ZeroTimed = 100,
            Low = 15,
            Normal = 10,
            High = 5
        }

        [Drivers.Compiler.Attributes.Group(Name = "IsolatedKernel_Hardware")]
        private static Scheduling.IScheduler TheScheduler;
        
        public static long PreemptionPeriod
        {
            get
            {
                return TheScheduler.PreemptionPeriod;
            }
        }
        public static FOS_System.IObject PreemptionState
        {
            get
            {
                return TheScheduler;
            }
        }

        public static void Init()
        {
            ExceptionMethods.ThePageFaultHandler = HandlePageFault;

            TheScheduler = new Scheduling.PriorityQueueScheduler();
            TheScheduler.Init();
        }
        public static Scheduling.PreemptionHandler Start()
        {
            return TheScheduler.Start();
        }


        public static void InitProcess(Process process, Scheduler.Priority priority)
        {
            TheScheduler.InitProcess(process, priority);
        }
        public static void InitThread(Process process, Thread t)
        {
            TheScheduler.InitThread(process, t);
        }
        public static void UpdateList(Thread t)
        {
            TheScheduler.UpdateList(t);
        }


        public static void UpdateCurrentState()
        {
            TheScheduler.UpdateCurrentState();
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Enable()
        {
            TheScheduler.Enable();
        }
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Disable()
        {
            TheScheduler.Disable();
        }
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static bool IsEnabled()
        {
            return TheScheduler.IsEnabled();
        }


        public static void HandlePageFault(uint eip, uint errorCode, uint address)
        {
            TheScheduler.HandlePageFault(eip, errorCode, address);
        }

        
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Processes\Scheduler")]
        public static void LoadTR()
        {
        }
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static unsafe TSS* GetTSSPointer()
        {
            return null;
        }
    }
    
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct TSS
    {
        /* For obvious reasons, do not reorder the fields. */

        public ushort backlink, __blh;
        public uint esp0;
        public ushort ss0, __ss0h;
        public uint esp1;
        public ushort ss1, __ss1h;
        public uint esp2;
        public ushort ss2, __ss2h;
        public uint cr3;
        public uint eip;
        public uint eflags;
        public uint eax, ecx, edx, ebx;
        public uint esp, ebp, esi, edi;
        public ushort es, __esh;
        public ushort cs, __csh;
        public ushort ss, __ssh;
        public ushort ds, __dsh;
        public ushort fs, __fsh;
        public ushort gs, __gsh;
        public ushort ldt, __ldth;
        public ushort trace, bitmap;
    }
}
