﻿/*
 * Copyright 2017 Mikhail Shiryaev
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
 * Summary  : The control for editing element
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2012
 * Modified : 2017
 */

using Scada.Comm.Devices.Modbus.Protocol;
using Scada.UI;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Scada.Comm.Devices.Modbus.UI
{
    /// <summary>
    /// The control for editing element
    /// <para>Элемент управления для редактирования элемента Modbus</para>
    /// </summary>
    public partial class CtrlElem : UserControl
    {
        private ElemInfo elemInfo;


        /// <summary>
        /// Конструктор
        /// </summary>
        public CtrlElem()
        {
            InitializeComponent();
            elemInfo = null;
        }


        /// <summary>
        /// Получить или установить редактируемый элемент
        /// </summary>
        public ElemInfo ElemInfo
        {
            get
            {
                return elemInfo;
            }
            set
            {
                elemInfo = null; // чтобы не вызывалось событие ObjectChanged
                ShowElemProps(value);
                elemInfo = value;
            }
        }


        /// <summary>
        /// Отобразить свойства элемента
        /// </summary>
        private void ShowElemProps(ElemInfo elemInfo)
        {
            if (elemInfo == null)
            {
                txtElemName.Text = "";
                txtElemAddress.Text = "";
                txtElemSignal.Text = "";
                rbBool.Checked = true;
                txtElemByteOrder.Text = "";
                gbElem.Enabled = false;
            }
            else
            {
                txtElemName.Text = elemInfo.Elem.Name;
                txtElemAddress.Text = elemInfo.AddressRange;
                txtElemSignal.Text = elemInfo.Signal.ToString();
                ElemTypes elemType = elemInfo.Elem.ElemType;

                if (elemInfo.ElemGroup.TableType != TableTypes.HoldingRegisters)
                {

                    if (elemType == ElemTypes.Bool)
                    {
                        rbUShort.Enabled = rbShort.Enabled = rbUInt.Enabled = rbInt.Enabled =
                            rbULong.Enabled = rbLong.Enabled = rbFloat.Enabled = rbDouble.Enabled = false;
                        rbBool.Enabled = true;
                        txtElemByteOrder.Text = "";
                        txtElemByteOrder.Enabled = false;
                    }
                    else
                    {
                        rbUShort.Enabled = rbShort.Enabled = rbUInt.Enabled = rbInt.Enabled =
                            rbULong.Enabled = rbLong.Enabled = rbFloat.Enabled = rbDouble.Enabled = true;
                        rbBool.Enabled = false;
                        txtElemByteOrder.Text = elemInfo.Elem.ByteOrderStr;
                        txtElemByteOrder.Enabled = true;
                    }
                }
                else
                {
                    rbUShort.Enabled = rbShort.Enabled = rbUInt.Enabled = rbInt.Enabled =
                            rbULong.Enabled = rbLong.Enabled = rbFloat.Enabled = rbDouble.Enabled = rbBool.Enabled = true;
                    txtElemByteOrder.Text = elemInfo.Elem.ByteOrderStr;
                    txtElemByteOrder.Enabled = true;
                    rbBool.CheckedChanged -= RbBool_CheckedChanged;
                    rbBool.CheckedChanged += RbBool_CheckedChanged;
                    lblElemByteOrder.Visible = !rbBool.Checked;
                    lblElemBitoffset.Visible = rbBool.Checked;
                }

                switch (elemType)
                {
                    case ElemTypes.UShort:
                        rbUShort.Checked = true;
                        break;
                    case ElemTypes.Short:
                        rbShort.Checked = true;
                        break;
                    case ElemTypes.UInt:
                        rbUInt.Checked = true;
                        break;
                    case ElemTypes.Int:
                        rbInt.Checked = true;
                        break;
                    case ElemTypes.ULong:
                        rbULong.Checked = true;
                        break;
                    case ElemTypes.Long:
                        rbLong.Checked = true;
                        break;
                    case ElemTypes.Float:
                        rbFloat.Checked = true;
                        break;
                    case ElemTypes.Double:
                        rbDouble.Checked = true;
                        break;
                    default:
                        rbBool.Checked = true;
                        break;
                }

                gbElem.Enabled = true;
            }
        }

        private void RbBool_CheckedChanged(object sender, EventArgs e)
        {
            lblElemByteOrder.Visible = !rbBool.Checked;
            lblElemBitoffset.Visible = rbBool.Checked;
        }

        /// <summary>
        /// Вызвать событие ObjectChanged
        /// </summary>
        private void OnObjectChanged(object changeArgument)
        {
            ObjectChanged?.Invoke(this, new ObjectChangedEventArgs(elemInfo, changeArgument));
        }

        /// <summary>
        /// Установить фокус ввода
        /// </summary>
        public void SetFocus()
        {
            txtElemName.Select();
        }


        /// <summary>
        /// Событие возникающее при изменении свойств редактируемого объекта
        /// </summary>
        [Category("Property Changed")]
        public event ObjectChangedEventHandler ObjectChanged;


        private void txtElemName_TextChanged(object sender, EventArgs e)
        {
            // изменение наименования элемента
            if (elemInfo != null)
            {
                elemInfo.Elem.Name = txtElemName.Text;
                OnObjectChanged(TreeUpdateTypes.CurrentNode);
            }
        }

        private void rbType_CheckedChanged(object sender, EventArgs e)
        {
            // изменение типа элемента
            if (elemInfo != null && ((RadioButton)sender).Checked)
            {
                Elem elem = elemInfo.Elem;

                if (rbUShort.Checked)
                    elem.ElemType = ElemTypes.UShort;
                else if (rbShort.Checked)
                    elem.ElemType = ElemTypes.Short;
                else if (rbUInt.Checked)
                    elem.ElemType = ElemTypes.UInt;
                else if (rbInt.Checked)
                    elem.ElemType = ElemTypes.Int;
                else if (rbULong.Checked)
                    elem.ElemType = ElemTypes.ULong;
                else if (rbLong.Checked)
                    elem.ElemType = ElemTypes.Long;
                else if (rbFloat.Checked)
                    elem.ElemType = ElemTypes.Float;
                else if (rbDouble.Checked)
                    elem.ElemType = ElemTypes.Double;
                else
                    elem.ElemType = ElemTypes.Bool;

                txtElemAddress.Text = elemInfo.AddressRange;
                OnObjectChanged(TreeUpdateTypes.CurrentNode | TreeUpdateTypes.NextSiblings);
            }
        }

        private void txtByteOrder_TextChanged(object sender, EventArgs e)
        {
            // изменение порядка байт элемента
            if (elemInfo != null)
            {
                elemInfo.Elem.ByteOrderStr = txtElemByteOrder.Text;
                OnObjectChanged(TreeUpdateTypes.None);
            }
        }
    }
}
