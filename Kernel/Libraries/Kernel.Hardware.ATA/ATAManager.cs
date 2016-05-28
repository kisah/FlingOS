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

using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.Devices.Controllers;
using Kernel.Devices;

namespace Kernel.ATA
{
    /// <summary>
    ///     Provides methods for managing ATA access.
    /// </summary>
    public static class ATAManager
    {
        /// <summary>
        ///     ATA primary IO device.
        /// </summary>
        private static ATAIOPorts ATAIO1;

        /// <summary>
        ///     ATA secondary IO device.
        /// </summary>
        private static ATAIOPorts ATAIO2;

        public static List Devices;

        /// <summary>
        ///     Initialises all available ATA devices on the primary bus.
        /// </summary>
        public static void Init()
        {
            Devices = new List(4);

            ATAIO1 = new ATAIOPorts(false);
            ATAIO2 = new ATAIOPorts(true);

            Exception ex = null;
            int exCount = 0;

            //Try to initialise primary IDE:PATA/PATAPI drives.
            try
            {
                InitDrive(ATA.ControllerID.Primary, ATA.BusPosition.Master);
            }
            catch
            {
                exCount++;
                if (exCount == 1)
                {
                    ex = ExceptionMethods.CurrentException;
                }
                else
                {
                    Exception newEx =
                        new Exception("Multiple errors occurred while initialising the ATA stack. Count=" +
                                      (String) exCount);
                    newEx.InnerException = ex;
                    ex = newEx;
                }
            }
            try
            {
                InitDrive(ATA.ControllerID.Primary, ATA.BusPosition.Slave);
            }
            catch
            {
                exCount++;
                if (exCount == 1)
                {
                    ex = ExceptionMethods.CurrentException;
                }
                else
                {
                    Exception newEx =
                        new Exception("Multiple errors occurred while initialising the ATA stack. Count=" +
                                      (String) exCount);
                    newEx.InnerException = ex;
                    ex = newEx;
                }
            }
            try
            {
                InitDrive(ATA.ControllerID.Secondary, ATA.BusPosition.Master);
            }
            catch
            {
                exCount++;
                if (exCount == 1)
                {
                    ex = ExceptionMethods.CurrentException;
                }
                else
                {
                    Exception newEx =
                        new Exception("Multiple errors occurred while initialising the ATA stack. Count=" +
                                      (String) exCount);
                    newEx.InnerException = ex;
                    ex = newEx;
                }
            }
            try
            {
                InitDrive(ATA.ControllerID.Secondary, ATA.BusPosition.Slave);
            }
            catch
            {
                exCount++;
                if (exCount == 1)
                {
                    ex = ExceptionMethods.CurrentException;
                }
                else
                {
                    Exception newEx =
                        new Exception("Multiple errors occurred while initialising the ATA stack. Count=" +
                                      (String) exCount);
                    newEx.InnerException = ex;
                    ex = newEx;
                }
            }

            if (exCount > 0)
            {
                ExceptionMethods.Throw(ex);
            }

            //TODO: Init SATA/SATAPI devices by enumerating PCI devices.
        }

        /// <summary>
        ///     Initialises a particular drive on the ATA bus.
        /// </summary>
        /// <param name="ctrlId">The controller ID of the device.</param>
        /// <param name="busPos">The bus position of the device.</param>
        public static void InitDrive(ATA.ControllerID ctrlId, ATA.BusPosition busPos)
        {
            //Get the IO ports for the correct bus
            ATAIOPorts theIO = ctrlId == ATA.ControllerID.Primary ? ATAIO1 : ATAIO2;
            //Create / init the device on the bus
            try
            {
                PATABase ThePATABase = new PATABase(theIO, ctrlId, busPos);
                //If the device was detected as present:
                if (ThePATABase.DriveType != PATABase.SpecLevel.Null)
                {
                    //If the device was actually a PATA device:
                    if (ThePATABase.DriveType == PATABase.SpecLevel.PATA)
                    {
                        //Add it to the list of devices.
                        try
                        {
                            PATA device = new PATA(ThePATABase);
                            Devices.Add(device);
                            DeviceManager.RegisterDevice(device);

                            // Initialise a thread to control the interface to the disk
                            StorageController.Init();
                            StorageController.AddDisk(device);
                        }
                        catch
                        {
                            ExceptionMethods.Throw(new Exception("Error initialising PATA device."));
                        }
                    }
                    else if (ThePATABase.DriveType == PATABase.SpecLevel.PATAPI)
                    {
                        // Add a PATAPI device
                        try
                        {
                            PATAPI device = new PATAPI(ThePATABase);
                            Devices.Add(device);
                            DeviceManager.RegisterDevice(device);

                            // Initialise a thread to control the interface to the disk
                            StorageController.Init();
                            StorageController.AddDisk(device);
                        }
                        catch
                        {
                            ExceptionMethods.Throw(new Exception("Error initialising PATAPI device."));
                        }
                    }
                    //TODO: Remove the SATA/SATAPI initialisation from here. It should be done
                    //  in the ATAManager.Init method when enumerating PCI bus for SATA/SATAPI devices.
                    else if (ThePATABase.DriveType == PATABase.SpecLevel.SATA)
                    {
                        // Add a SATA device
                        try
                        {
                            SATA device = new SATA();
                            Devices.Add(device);
                            DeviceManager.RegisterDevice(device);

                            // TODO: Initialise a thread to control the interface to the disk (SATA)
                            //Controllers.StorageController.Init();
                            //Controllers.StorageController.AddDisk(device);
                        }
                        catch
                        {
                            ExceptionMethods.Throw(new Exception("Error initialising SATA device."));
                        }
                    }
                    else if (ThePATABase.DriveType == PATABase.SpecLevel.SATAPI)
                    {
                        // Add a SATAPI device
                        try
                        {
                            SATAPI device = new SATAPI();
                            Devices.Add(device);
                            DeviceManager.RegisterDevice(device);

                            // TODO: Initialise a thread to control the interface to the disk (SATAPI)
                            //Controllers.StorageController.Init();
                            //Controllers.StorageController.AddDisk(device);
                        }
                        catch
                        {
                            ExceptionMethods.Throw(new Exception("Error initialising SATAPI device."));
                        }
                    }
                }
            }
            catch
            {
                ExceptionMethods.Throw(new Exception((String) "Error initialising PATA Base device. Controller ID: " +
                                                     (ctrlId == ATA.ControllerID.Primary ? "Primary" : "Secondary") +
                                                     " , Position: " +
                                                     (busPos == ATA.BusPosition.Master ? "Master" : "Slave")));
            }
        }
    }
}