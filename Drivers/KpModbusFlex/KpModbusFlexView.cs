﻿/*
 * Copyright 2018 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : KpModbus
 * Summary  : Device library user interface
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2012
 * Modified : 2018
 */

using Scada.Comm.Devices.Modbus.Protocol;
using Scada.Comm.Devices.Modbus.UI;
using Scada.Data.Configuration;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scada.Comm.Devices
{
    /// <summary>
    /// Device library user interface
    /// <para>Пользовательский интерфейс библиотеки КП</para>
    /// </summary>
    public class KpModbusFlexView : KPView
    {
        /// <summary>
        /// Версия библиотеки КП
        /// </summary>
        internal const string KpVersion = "5.1.0.2";


        /// <summary>
        /// Конструктор для общей настройки библиотеки КП
        /// </summary>
        public KpModbusFlexView()
            : this(0)
        {
        }

        /// <summary>
        /// Конструктор для настройки конкретного КП
        /// </summary>
        public KpModbusFlexView(int number)
            : base(number)
        {
            CanShowProps = true;
        }


        /// <summary>
        /// Описание библиотеки КП
        /// </summary>
        public override string KPDescr
        {
            get
            {
                return Localization.UseRussian ? 
                    "Взаимодействие с контроллерами по протоколу Modbus.\n\n" +
                    "Пользовательский параметр линии связи:\n" +
                    "TransMode - режим передачи данных (RTU, ASCII, TCP).\n\n" + 
                    "Параметр командной строки:\n" +
                    "имя файла шаблона.\n\n" +
                    "Команды ТУ:\n" +
                    "определяются шаблоном (стандартные или бинарные)." :

                    "Interacting with controllers via Modbus protocol.\n\n" +
                    "Custom communication line parameter:\n" +
                    "TransMode - data transmission mode (RTU, ASCII, TCP).\n\n" +
                    "Command line parameter:\n" +
                    "template file name.\n\n" +
                    "Commands:\n" +
                    "defined by template (standard or binary).\n\n" +
                    "get bits from byte of holding registers";
            }
        }

        /// <summary>
        /// Получить версию библиотеки КП
        /// </summary>
        public override string Version
        {
            get
            {
                return KpVersion;
            }
        }

        /// <summary>
        /// Получить прототипы каналов КП по умолчанию
        /// </summary>
        public override KPCnlPrototypes DefaultCnls
        {
            get
            {
                // получение имени файла шаблона устройства
                string templateName = KPProps == null ? "" : KPProps.CmdLine.Trim();
                string fileName = AppDirs.ConfigDir + templateName;
                if (!File.Exists(fileName))
                    return null;

                // загрузка шаблона устройства
                DeviceTemplate template = new DeviceTemplate();
                string errMsg;
                if (!template.Load(fileName, out errMsg))
                    throw new Exception(errMsg);

                // создание прототипов каналов КП
                KPCnlPrototypes prototypes = new KPCnlPrototypes();
                List<InCnlPrototype> inCnls = prototypes.InCnls;
                List<CtrlCnlPrototype> ctrlCnls = prototypes.CtrlCnls;

                // создание прототипов входных каналов
                int signal = 1;
                foreach (ElemGroup elemGroup in template.ElemGroups)
                {
                    bool isTS = 
                        elemGroup.TableType == TableTypes.DiscreteInputs || 
                        elemGroup.TableType == TableTypes.Coils;

                    foreach (Elem elem in elemGroup.Elems)
                    {
                        InCnlPrototype inCnl = isTS ?
                            new InCnlPrototype(elem.Name, BaseValues.CnlTypes.TS) :
                            new InCnlPrototype(elem.Name, BaseValues.CnlTypes.TI);
                        inCnl.Signal = signal++;

                        if (isTS)
                        {
                            inCnl.ShowNumber = false;
                            inCnl.UnitName = BaseValues.UnitNames.OffOn;
                            inCnl.EvEnabled = true;
                            inCnl.EvOnChange = true;
                        }

                        inCnls.Add(inCnl);
                    }
                }

                // создание прототипов каналов управления
                foreach (ModbusCmd modbusCmd in template.Cmds)
                {
                    CtrlCnlPrototype ctrlCnl = modbusCmd.TableType == TableTypes.Coils && modbusCmd.Multiple ?
                        new CtrlCnlPrototype(modbusCmd.Name, BaseValues.CmdTypes.Binary) :
                        new CtrlCnlPrototype(modbusCmd.Name, BaseValues.CmdTypes.Standard);
                    ctrlCnl.CmdNum = modbusCmd.CmdNum;

                    if (modbusCmd.TableType == TableTypes.Coils && !modbusCmd.Multiple)
                        ctrlCnl.CmdValName = BaseValues.CmdValNames.OffOn;

                    ctrlCnls.Add(ctrlCnl);
                }

                return prototypes;
            }
        }


        /// <summary>
        /// Отобразить свойства КП
        /// </summary>
        public override void ShowProps()
        {
            // загрузка словарей
            string errMsg;
            if (Localization.LoadDictionaries(AppDirs.LangDir, "KpModbus", out errMsg))
                KpPhrases.Init();
            else
                ScadaUiUtils.ShowError(errMsg);

            if (Number > 0)
                // отображение свойств КП
                FrmDevProps.ShowDialog(Number, KPProps, AppDirs);
            else
                // отображение редактора шаблонов устройств
                FrmDevTemplate.ShowDialog(AppDirs);
        }
    }
}
