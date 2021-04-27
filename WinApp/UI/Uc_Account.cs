using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Core.Models;

namespace WinApp.UI
{
    public partial class Uc_Account : UserControl
    {
        AccountSettings _accountSettings;

        Label lblAccount;
        Label lblSymbol;
        Label lblPosition;

        public Uc_Account()
        {
            InitializeComponent();

            this.tableLayoutPanel1.Controls.Add(CreateLabel("帳號：", ContentAlignment.MiddleRight), 0, 0);
            lblAccount = CreateLabel("", ContentAlignment.MiddleLeft);
            this.tableLayoutPanel1.Controls.Add(lblAccount, 1, 0);

            this.tableLayoutPanel1.Controls.Add(CreateLabel("商品：", ContentAlignment.MiddleRight), 2, 0);
            lblSymbol = CreateLabel("", ContentAlignment.MiddleLeft);
            this.tableLayoutPanel1.Controls.Add(lblSymbol, 3, 0);

            this.tableLayoutPanel1.Controls.Add(CreateLabel("口數：", ContentAlignment.MiddleRight), 4, 0);
            lblPosition = CreateLabel("", ContentAlignment.MiddleLeft);
            this.tableLayoutPanel1.Controls.Add(lblPosition, 5, 0);
        }

        public void BindData(AccountSettings accountSettings)
        {
            _accountSettings = accountSettings;
            lblAccount.Text = accountSettings.Account;
            lblSymbol.Text = accountSettings.Symbol;
            lblPosition.Text = accountSettings.Lot.ToString();
        }

        Label CreateLabel(string text, ContentAlignment alignment = ContentAlignment.MiddleLeft)
        {
            var lbl = new Label();
            lbl.TextAlign = alignment;
            lbl.Text = text;
            return lbl;
        }
    }
}
