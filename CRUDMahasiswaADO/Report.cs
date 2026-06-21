using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;

namespace CRUDMahasiswaADO
{
    public partial class Report : Form
    {
        // Gunakan readonly atau private biasa untuk connection string
        private static string connectionString = "Data Source=KAIDEN\\BLAZE;Initial Catalog=DBAkademikADO;User ID=sa;Password=towinnadzul09122005";

        private SqlConnection conn = new SqlConnection(connectionString);
        private SqlDataAdapter da;
        private DataTable dtMahasiswa;

        // PERBAIKAN DI SINI: Ubah nama objek variabelnya menjadi 'rptMahasiswa' (huruf kecil/berbeda)
        // agar tidak bentrok dengan nama Class 'LaporanMahasiswa'
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
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inProdi", prodi);
                cmd.Parameters.AddWithValue("@inTglMsuk", tglmasuk.Year);

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);
                // Selalu tutup koneksi setelah selesai mengambil data
                conn.Close();

                MessageBox.Show("Jumlah baris: " + dtMahasiswa.Rows.Count);
                if (dtMahasiswa.Rows.Count > 0)
                {
                    MessageBox.Show("Data pertama - Nama: " + dtMahasiswa.Rows[0]["Nama"].ToString());
                }

                // PERBAIKAN DI SINI: Panggil menggunakan nama objek variabel yang baru (rptMahasiswa)
                rptMahasiswa.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = rptMahasiswa;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                // Pastikan koneksi ditutup jika terjadi error di tengah jalan
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                MessageBox.Show("Gagal Load data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {
            // Bisa dikosongkan jika tidak dipakai
        }
    }
}