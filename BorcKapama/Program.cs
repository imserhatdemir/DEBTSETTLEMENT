using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;



public class Transaction
{
    public DateTime Tarih { get; set; }
    public decimal Tutar { get; set; }
    public decimal Kalan { get; set; }

    public Transaction(DateTime tarih, decimal tutar)
    {
        Tarih = tarih;
        Tutar = tutar;
        Kalan = tutar;
    }
}
public class Match
{
    public Transaction BorcIslem { get; set; }
    public Transaction AlacakIslem { get; set; }
    public decimal KapatilanTutar { get; set; }

    public Match(Transaction borcIslem, Transaction alacakIslem, decimal kapatilanTutar)
    {
        BorcIslem = borcIslem;
        AlacakIslem = alacakIslem;
        KapatilanTutar = kapatilanTutar;
    }
}
class Program
{
    const String connectionString = "Data Source=DESKTOP-CPG75NF\\SQLEXPRESS;Initial Catalog=borckapama;Integrated Security=True;";

    
    public static List<Transaction> GetBorclar()
    {
        var list = new List<Transaction>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (SqlCommand cmd = new SqlCommand("SELECT Tarih, Borc FROM borctablosu WHERE Borc IS NOT NULL", connection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Transaction(reader.GetDateTime(0), (decimal)reader.GetInt32(1)));
                    }
                }
            }
        }

        return list;
    }

    public static List<Transaction> GetAlacaklar()
    {
        var list = new List<Transaction>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (SqlCommand cmd = new SqlCommand("SELECT Tarih, Alacak FROM borctablosu WHERE Alacak IS NOT NULL", connection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Transaction(reader.GetDateTime(0), (decimal)reader.GetInt32(1)));
                    }
                }
            }
        }

        return list;
    }

    static void Main()
    {
        var borclar = GetBorclar();
        var alacaklar = GetAlacaklar();

        var eslesmeler = new CreateMatching().Matching(borclar, alacaklar);

        foreach (var eslesme in eslesmeler)
        {
            Console.WriteLine($"Borc: {eslesme.BorcIslem.Tarih.ToShortDateString()} - {eslesme.BorcIslem.Tutar} => Alacak: {eslesme.AlacakIslem.Tarih.ToShortDateString()} - {eslesme.AlacakIslem.Tutar} (Kapatılan: {eslesme.KapatilanTutar})");
        }

        Console.ReadLine();
    }
}
public class CreateMatching
{
    public List<Match> Matching(List<Transaction> borclar, List<Transaction> alacaklar)
    {
        List<Match> eslesmeler = new List<Match>();

        foreach (var borc in borclar)
        {
            foreach (var alacak in alacaklar)
            {
                if (borc.Kalan > 0 && alacak.Kalan > 0)
                {
                    decimal kapatilan = Math.Min(borc.Kalan, alacak.Kalan);
                    eslesmeler.Add(new Match(borc, alacak, kapatilan));

                    borc.Kalan -= kapatilan;
                    alacak.Kalan -= kapatilan;
                }
            }
        }

        return eslesmeler;
    }
}

