using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class RekapMahasiswa : Form
    {
        DAL dbLogic = new DAL();


        DataTable dtMahasiswa; 

        public RekapMahasiswa()
        {
            InitializeComponent();
        }

        private void Form2_Load_1(object sender, EventArgs e)
        {
            dtpTanggalMasuk.Format = DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "yyyy";
            dtpTanggalMasuk.ShowUpDown = true;
            dtpTanggalMasuk.MinDate = new DateTime(2000, 1, 1);
            dtpTanggalMasuk.MaxDate = DateTime.Now;

            cmbProdi.DropDownStyle = ComboBoxStyle.DropDownList;
            btnCetak.Enabled = false;

            try
            {
                // Pakai DAL untuk load prodi
                DataTable dtProdi = dbLogic.getProdi();
                cmbProdi.DataSource = dtProdi;
                cmbProdi.DisplayMember = "namaprodi";
                cmbProdi.ValueMember = "namaprodi";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void btnLoad_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Pakai DAL untuk load rekap
                dtMahasiswa = dbLogic.getDataRekap(
                    cmbProdi.SelectedValue.ToString(),
                    dtpTanggalMasuk.Value);

                dataGridView1.DataSource = dtMahasiswa;

                if (dtMahasiswa.Rows.Count > 0)
                {
                    btnCetak.Enabled = true;
                }
                else
                {
                    btnCetak.Enabled = false;
                    MessageBox.Show("Data tidak ditemukan");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void btnCetak_Click_1(object sender, EventArgs e)
        {
            Report frm2 = new Report(
                cmbProdi.SelectedValue.ToString(),
                dtpTanggalMasuk.Value);
            frm2.Show();
            this.Hide();
        }
    }
}