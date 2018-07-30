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
 * Summary  : The phrases used in the library
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2014
 * Modified : 2018
 */

namespace Scada.Comm.Devices.Modbus.UI
{
    /// <summary>
    /// The phrases used in the library
    /// <para>Фразы, используемые библиотекой</para>
    /// </summary>
    internal static class KpPhrases
    {
        static KpPhrases()
        {
            SetToDefault();
        }

        // Словарь Scada.Comm.Devices.Modbus.UI.FrmDevTemplate
        public static string TemplFormTitle { get; private set; }
        public static string GrsNode { get; private set; }
        public static string CmdsNode { get; private set; }
        public static string DefGrName { get; private set; }
        public static string DefElemName { get; private set; }
        public static string DefCmdName { get; private set; }
        public static string AddressHint { get; private set; }
        public static string SaveTemplateConfirm { get; private set; }
        public static string ElemCntExceeded { get; private set; }
        public static string ElemRemoveWarning { get; private set; }
        public static string TemplateFileFilter { get; private set; }

        // Словарь Scada.Comm.Devices.Modbus.UI.FrmDevProps
        public static string TemplNotExists { get; private set; }

        private static void SetToDefault()
        {
            TemplFormTitle = Localization.Dict.GetEmptyPhrase("TemplFormTitle");
            GrsNode = Localization.Dict.GetEmptyPhrase("GrsNode");
            CmdsNode = Localization.Dict.GetEmptyPhrase("CmdsNode");
            DefGrName = Localization.Dict.GetEmptyPhrase("DefGrName");
            DefElemName = Localization.Dict.GetEmptyPhrase("DefElemName");
            DefCmdName = Localization.Dict.GetEmptyPhrase("DefCmdName");
            AddressHint = Localization.Dict.GetEmptyPhrase("AddressHint");
            SaveTemplateConfirm = Localization.Dict.GetEmptyPhrase("SaveTemplateConfirm");
            ElemCntExceeded = Localization.Dict.GetEmptyPhrase("ElemCntExceeded");
            ElemRemoveWarning = Localization.Dict.GetEmptyPhrase("ElemRemoveWarning");
            TemplateFileFilter = Localization.Dict.GetEmptyPhrase("TemplateFileFilter");

            TemplNotExists = Localization.Dict.GetEmptyPhrase("TemplNotExists");
        }

        public static void Init()
        {
            Localization.Dict dict;
            if (Localization.Dictionaries.TryGetValue("Scada.Comm.Devices.Modbus.UI.FrmDevTemplate", out dict))
            {
                TemplFormTitle = dict.GetPhrase("this", TemplFormTitle);
                GrsNode = dict.GetPhrase("GrsNode", GrsNode);
                CmdsNode = dict.GetPhrase("CmdsNode", CmdsNode);
                DefGrName = dict.GetPhrase("DefGrName", DefGrName);
                DefElemName = dict.GetPhrase("DefElemName", DefElemName);
                DefCmdName = dict.GetPhrase("DefCmdName", DefCmdName);
                AddressHint = dict.GetPhrase("AddressHint", AddressHint);
                SaveTemplateConfirm = dict.GetPhrase("SaveTemplateConfirm", SaveTemplateConfirm);
                ElemCntExceeded = dict.GetPhrase("ElemCntExceeded", ElemCntExceeded);
                ElemRemoveWarning = dict.GetPhrase("ElemRemoveWarning", ElemRemoveWarning);
                TemplateFileFilter = dict.GetPhrase("TemplateFileFilter", TemplateFileFilter);
            }

            if (Localization.Dictionaries.TryGetValue("Scada.Comm.Devices.Modbus.UI.FrmDevProps", out dict))
            {
                TemplNotExists = dict.GetPhrase("TemplNotExists", TemplNotExists);
            }
        }
    }
}
