using System;
using System.Data;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;

namespace CRUDMahasiswaADO
{
    public partial class Report : Form
    {
        // Tambahkan DAL di sini
        DAL dbLogic = new DAL();

        private LaporanMahasiswa rptMahasiswa = new LaporanMahasiswa();

        string prodi { get; set; }
        DateTime tglmasuk { get; set; }

        public Report(string Prodi, DateTime TglMasuk)
        {
            InitializeComponent();

            prodi = Prodi;
            tglmasuk = TglMasuk;

            try
            {
                // Pakai dbLogic yang sudah dideklarasikan
                DataTable dtMahasiswa = dbLogic.getDataRekap(prodi, tglmasuk);

                // Pakai rptMahasiswa bukan listMahasiswa
                rptMahasiswa.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = rptMahasiswa;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {
            // Kosongkan saja
        }
    }
}