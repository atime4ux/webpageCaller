namespace webpageCaller
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.MainCallerState = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.RetryCallerState = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Tahoma", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(20, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(262, 70);
            this.button1.TabIndex = 1;
            this.button1.Text = "Webpage Caller";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Main Caller -";
            // 
            // MainCallerState
            // 
            this.MainCallerState.AutoSize = true;
            this.MainCallerState.Location = new System.Drawing.Point(104, 102);
            this.MainCallerState.Name = "MainCallerState";
            this.MainCallerState.Size = new System.Drawing.Size(38, 12);
            this.MainCallerState.TabIndex = 3;
            this.MainCallerState.Text = "label2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Retry Caller -";
            // 
            // RetryCallerState
            // 
            this.RetryCallerState.AutoSize = true;
            this.RetryCallerState.Location = new System.Drawing.Point(105, 124);
            this.RetryCallerState.Name = "RetryCallerState";
            this.RetryCallerState.Size = new System.Drawing.Size(38, 12);
            this.RetryCallerState.TabIndex = 5;
            this.RetryCallerState.Text = "label4";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 145);
            this.Controls.Add(this.RetryCallerState);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.MainCallerState);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Webpage Caller";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label MainCallerState;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label RetryCallerState;
    }
}

