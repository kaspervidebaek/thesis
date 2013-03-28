namespace WindowApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.leftTree = new System.Windows.Forms.TreeView();
            this.rightTree = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bottomTree = new System.Windows.Forms.TreeView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.baseTree = new System.Windows.Forms.TreeView();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // leftTree
            // 
            this.leftTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftTree.Location = new System.Drawing.Point(0, 0);
            this.leftTree.Name = "leftTree";
            this.leftTree.Size = new System.Drawing.Size(421, 371);
            this.leftTree.TabIndex = 0;
            // 
            // rightTree
            // 
            this.rightTree.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightTree.Location = new System.Drawing.Point(759, 0);
            this.rightTree.Name = "rightTree";
            this.rightTree.Size = new System.Drawing.Size(410, 371);
            this.rightTree.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bottomTree);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 371);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1169, 211);
            this.panel1.TabIndex = 3;
            // 
            // bottomTree
            // 
            this.bottomTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomTree.Location = new System.Drawing.Point(0, 0);
            this.bottomTree.Name = "bottomTree";
            this.bottomTree.Size = new System.Drawing.Size(1169, 211);
            this.bottomTree.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.baseTree);
            this.panel2.Controls.Add(this.leftTree);
            this.panel2.Controls.Add(this.rightTree);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1169, 371);
            this.panel2.TabIndex = 4;
            // 
            // baseTree
            // 
            this.baseTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseTree.Location = new System.Drawing.Point(421, 0);
            this.baseTree.Name = "baseTree";
            this.baseTree.Size = new System.Drawing.Size(338, 371);
            this.baseTree.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1169, 582);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView leftTree;
        private System.Windows.Forms.TreeView rightTree;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TreeView bottomTree;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TreeView baseTree;


    }
}

