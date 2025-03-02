﻿using ASTRALib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;

namespace поиск_недостоверной_ТМ_по_корреляции
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.BufferHeight = 30000;
            string mainDir = "C:\\Users\\User\\Desktop\\учеба\\магистратура\\ВКР\\суточные срезы\\105 срезов для БД\\2023_01_11";
            var mainDirInfo = new DirectoryInfo(mainDir);
            DirectoryInfo[] subDirs = mainDirInfo.GetDirectories();

            var znachForTm = new Dictionary<TMKey, TwoList>();
            var slicesList = new List<Slices>();
            var activePowerImbalanceList = new List<ActivePowerImbalance>();
            var reactivePowerImbalanceList = new List<ReactivePowerImbalance>();

            int activeImbalanceOrderIndex = 1;
            int reactiveImbalanceOrderIndex = 1;
            foreach (DirectoryInfo subDir in subDirs)
            {
                ProcessSubDirectory(subDir, znachForTm, slicesList, activePowerImbalanceList, reactivePowerImbalanceList, activeImbalanceOrderIndex, reactiveImbalanceOrderIndex);
                activeImbalanceOrderIndex += 1;
                reactiveImbalanceOrderIndex += 1;
                // Освобождение ресурсов
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            CategorizeAndSaveTelemetryData(znachForTm, slicesList, activePowerImbalanceList, reactivePowerImbalanceList);
            Console.WriteLine("Усе");
        }

        static void ProcessSubDirectory(DirectoryInfo subDir, Dictionary<TMKey, TwoList> znachForTm, List<Slices> slicesList, List<ActivePowerImbalance> activePowerImbalanceList, List<ReactivePowerImbalance> reactivePowerImbalanceList, int activeImbalanceOrderIndex, int reactiveImbalanceOrderIndex)
        {
            Regex regex = new Regex(@"\b(\d\d_\d\d_\d\d)");
            Match match1 = regex.Match(Convert.ToString(subDir));
            FileInfo[] pathFile = subDir.GetFiles("roc_debug_after_OC*");

            if (pathFile.Length == 0)
            {
                Console.WriteLine($"Файлы не найдены в директории: {subDir.FullName}");
                return;
            }

            Rastr _rastr = new Rastr();
            string filePath = pathFile[0].FullName;
            _rastr.Load(RG_KOD.RG_REPL, filePath, "");

            // Обращение к таблице ТИ: Каналы
            ITable _tableTIСhannel = (ITable)_rastr.Tables.Item("ti");
            ICol measuredValues = (ICol)_tableTIСhannel.Cols.Item("ti_val");
            ICol estimatedValues = (ICol)_tableTIСhannel.Cols.Item("ti_ocen");
            ICol _typeTM = (ICol)_tableTIСhannel.Cols.Item("type");
            ICol _cod_v_OC = (ICol)_tableTIСhannel.Cols.Item("cod_oc");
            ICol tmIndex = (ICol)_tableTIСhannel.Cols.Item("Num");
            ICol lagrangeValues = (ICol)_tableTIСhannel.Cols.Item("lagr");
            ICol privyazka = (ICol)_tableTIСhannel.Cols.Item("prv_num");
            ICol key1 = (ICol)_tableTIСhannel.Cols.Item("id1");
            ICol key2 = (ICol)_tableTIСhannel.Cols.Item("id2");
            ICol key3 = (ICol)_tableTIСhannel.Cols.Item("id3");
            ICol deltaIzmOcen = (ICol)_tableTIСhannel.Cols.Item("dif_oc");
            ICol nameTM = (ICol)_tableTIСhannel.Cols.Item("name");

            // Обращение к таблице ТИ: Балансы P
            ITable _active_power_imbalance = (ITable)_rastr.Tables.Item("ti_balans_p");
            ICol n_nach_P = (ICol)_active_power_imbalance.Cols.Item("ti_ip");
            ICol n_kon_P = (ICol)_active_power_imbalance.Cols.Item("ti_iq");
            ICol name_P = (ICol)_active_power_imbalance.Cols.Item("name");
            ICol dP = (ICol)_active_power_imbalance.Cols.Item("dp");

            int countNebP = _active_power_imbalance.Size;
            for (int n_neb_p = 0; n_neb_p<countNebP; n_neb_p++)
            {
                int nNachPNeb = (int)n_nach_P.get_ZN(n_neb_p);
                int nKonPNeb = (int)n_kon_P.get_ZN(n_neb_p);
                string namePNeb = (string)name_P.get_ZN(n_neb_p);
                double dPNeb = (double)dP.get_ZN(n_neb_p);

                activePowerImbalanceList.Add(new ActivePowerImbalance
                {
                    ID = Guid.NewGuid(),
                    n_nach_p = nNachPNeb,
                    n_kon_p = nKonPNeb,
                    name_p = namePNeb,
                    p_neb_p = dPNeb,
                    SliceID_p = GetOrCreateSliceID(filePath, match1.Value, slicesList),
                    orderIndexP = activeImbalanceOrderIndex
                });
            }

            // Обращение к таблице ТИ: Балансы Q
            ITable _reactive_power_imbalance = (ITable)_rastr.Tables.Item("ti_balans_q");
            ICol n_nach_Q = (ICol)_reactive_power_imbalance.Cols.Item("ti_ip");
            ICol n_kon_Q = (ICol)_reactive_power_imbalance.Cols.Item("ti_iq");
            ICol name_Q = (ICol)_reactive_power_imbalance.Cols.Item("name");
            ICol dq = (ICol)_reactive_power_imbalance.Cols.Item("dq");

            int countNebQ = _reactive_power_imbalance.Size;

            for (int n_neb_q = 0; n_neb_q < countNebQ; n_neb_q++)
            {
                int nNachQNeb = (int)n_nach_Q.get_ZN(n_neb_q);
                int nKonQNeb = (int)n_kon_Q.get_ZN(n_neb_q);
                string nameQNeb = (string)name_Q.get_ZN(n_neb_q);
                double dQNeb = (double)dq.get_ZN(n_neb_q);

                reactivePowerImbalanceList.Add(new ReactivePowerImbalance
                {
                    ID = Guid.NewGuid(),
                    n_nach_q = nNachQNeb,
                    n_kon_q = nKonQNeb,
                    name_q = nameQNeb,
                    q_neb_q = dQNeb,
                    SliceID_q = GetOrCreateSliceID(filePath, match1.Value, slicesList),
                    orderIndexQ = reactiveImbalanceOrderIndex
                });
            }

            int countTM = _tableTIСhannel.Size;

            for (int numTm = 0; numTm < countTM; numTm++)
            {
                if (IsRelevantTM(_typeTM, _cod_v_OC, numTm))
                {
                    double index = (double)tmIndex.get_ZN(numTm);
                    double measured = (double)measuredValues.get_ZN(numTm);
                    double estimated = (double)estimatedValues.get_ZN(numTm);
                    double lagrange = (double)lagrangeValues.get_ZN(numTm);
                    string priv = (string)privyazka.get_ZN(numTm);
                    int id1 = (int)key1.get_ZN(numTm);
                    int id2 = (int)key2.get_ZN(numTm);
                    int id3 = (int)key3.get_ZN(numTm);
                    double delta = (double)deltaIzmOcen.get_ZN(numTm);
                    string name = (string)nameTM.get_ZN(numTm);
                    var tmKey = new TMKey(index, id1, id2, id3);
                    AddToDictionary(znachForTm, tmKey, measured, estimated, lagrange, priv, delta, name, match1.Value, filePath, slicesList);
                }
            }

            Console.WriteLine($"срез: {match1}");
        }

        static bool IsRelevantTM(ICol typeTM, ICol cod_v_OC, int numTm)
        {
            return (((int)typeTM.get_ZN(numTm) == 0) || ((int)typeTM.get_ZN(numTm) == 2)) && ((int)cod_v_OC.get_ZN(numTm) == 1);
        }

        static void AddToDictionary(Dictionary<TMKey, TwoList> znachForTm, TMKey key, double measured, double estimated, double lagrange, string priv, double delta, string nameTM, string match1, string filePath, List<Slices> slicesList)
        {
            Guid sliceID;
            if (!slicesList.Any(slice => slice.SlicePath == filePath))
            {
                sliceID = Guid.NewGuid();
                slicesList.Add(new Slices
                {
                    SliceID = sliceID,
                    SliceName = match1,
                    SlicePath = filePath
                });
            }
            else
            {
                sliceID = slicesList.First(slice => slice.SlicePath == filePath).SliceID;
            }

            if (!znachForTm.ContainsKey(key))
            {
                var twoList = new TwoList(new List<double> { measured }, new List<double> { estimated }, new List<double> { lagrange }, new List<string> { priv }, new List<double> { delta }, new List<string> { nameTM }, new List<string> { match1 });
                twoList.SliceIDs.Add(sliceID);  // Добавляем SliceID
                znachForTm[key] = twoList;
            }
            else
            {
                znachForTm[key].MeasuredValues.Add(measured);
                znachForTm[key].EstimatedValues.Add(estimated);
                znachForTm[key].LagrangeValues.Add(lagrange);
                znachForTm[key].PrivyazkaTM.Add(priv);
                znachForTm[key].DeltaIzmOc.Add(delta);
                znachForTm[key].Names.Add(nameTM);
                znachForTm[key].Srez.Add(match1);
                znachForTm[key].SliceIDs.Add(sliceID);  // Добавляем SliceID
            }
        }

        static Guid GetOrCreateSliceID(string slicePath, string sliceName, List<Slices> slicesList)
        {
            var existingSlice = slicesList.FirstOrDefault(s => s.SlicePath == slicePath);
            if (existingSlice != null)
            {
                return existingSlice.SliceID;
            }

            var newSlice = new Slices
            {
                SliceID = Guid.NewGuid(),
                SliceName = sliceName,
                SlicePath = slicePath
            };
            slicesList.Add(newSlice);
            return newSlice.SliceID;
        }

        static void CategorizeAndSaveTelemetryData(Dictionary<TMKey, TwoList> znachForTm, List<Slices> slicesList, List<ActivePowerImbalance> activePowerImbalanceList, List<ReactivePowerImbalance> reactivePowerImbalanceList)
        {
            var unreliableTM = new Dictionary<TMKey, double>();
            var questionableTM = new Dictionary<TMKey, double>();
            var reliableTM = new Dictionary<TMKey, double>();

            using (ApplicationContext db = new ApplicationContext())
            {
                //db.Database.ExecuteSqlRaw("TRUNCATE TABLE \"tm\" RESTART IDENTITY;");
                //db.Database.ExecuteSqlRaw("TRUNCATE TABLE \"tm\";");
                //db.Database.ExecuteSqlRaw("DELETE FROM \"TMValues\";");
                db.Database.ExecuteSqlRaw("TRUNCATE TABLE \"slices\" RESTART IDENTITY;");
                db.Database.ExecuteSqlRaw("TRUNCATE TABLE \"active_power_imbalance\" RESTART IDENTITY;");
                db.Database.ExecuteSqlRaw("TRUNCATE TABLE \"reactive_power_imbalance\" RESTART IDENTITY;");

                var tmValueEntries = new List<telemetryValues>();
                var tmEntries = new List<telemetry>();

                int orderIndex = 0;

                foreach (var entry in znachForTm)
                {
                    double correlation = CalculateCorrelation(entry.Value.MeasuredValues, entry.Value.EstimatedValues);
                    string status = DetermineStatus(correlation);

                    for (int i = 0; i < entry.Value.MeasuredValues.Count; i++)
                    {
                        tmValueEntries.Add(new telemetryValues
                        {
                            ID = Guid.NewGuid(),
                            IndexTM = entry.Key.Index,
                            IzmerValue = entry.Value.MeasuredValues[i],
                            OcenValue = entry.Value.EstimatedValues[i],
                            OrderIndex = orderIndex++,
                            Privyazka = entry.Value.PrivyazkaTM[i],
                            Id1 = entry.Key.Id1,
                            Id2 = entry.Key.Id2,
                            Id3 = entry.Key.Id3,
                            DeltaOcenIzmer = entry.Value.DeltaIzmOc[i],
                            NameTM = entry.Value.Names[i],
                            NumberOfSrez = entry.Value.Srez[i],
                            Correlation = Math.Round(correlation, 3),
                            Status = status,
                            SliceID = entry.Value.SliceIDs[i]  // Устанавливаем ссылку на SliceID
                        });
                    }


                    if (!double.IsNaN(correlation))
                    {
                        CategorizeTM(entry.Key, correlation, unreliableTM, questionableTM, reliableTM, db, entry.Value);
                        double maxPositiveLagrange = entry.Value.LagrangeValues.Where(x => x > 0).DefaultIfEmpty(0).Max();
                        double maxNegativeLagrange = entry.Value.LagrangeValues.Where(x => x < 0).DefaultIfEmpty(0).Min();
                        double avgLagrange = entry.Value.LagrangeValues.Average();
                        double maxAbsoluteLagrange = Math.Abs(maxPositiveLagrange) > Math.Abs(maxNegativeLagrange) ? maxPositiveLagrange : maxNegativeLagrange;

                        tmEntries.Add(new telemetry
                        {
                            ID = Guid.NewGuid(),
                            IndexTm = entry.Key.Index,
                            CorrTm = Math.Round(correlation, 3),
                            Status = status,
                            MaxLagranj = Math.Round(maxAbsoluteLagrange, 2),
                            AvgLagranj = Math.Round(avgLagrange, 2),
                            NameTM = entry.Value.Names[0]
                        });
                    }
                }

                //db.TMValues.AddRange(tmValueEntries);
               // db.tm.AddRange(tmEntries);
                db.slices.AddRange(slicesList);
                db.active_power_imbalance.AddRange(activePowerImbalanceList);
                db.reactive_power_imbalance.AddRange(reactivePowerImbalanceList);
                db.SaveChanges();
            }

            Console.WriteLine($"Не надежные ТМ: {unreliableTM.Count}");
            Console.WriteLine($"Спорные ТМ: {questionableTM.Count}");
            Console.WriteLine($"Надежные ТМ: {reliableTM.Count}");
        }


        static void CategorizeTM(TMKey key, double correlation, Dictionary<TMKey, double> unreliableTM, Dictionary<TMKey, double> questionableTM, Dictionary<TMKey, double> reliableTM, ApplicationContext db, TwoList values)
        {
            double maxPositiveLagrange = values.LagrangeValues.Where(x => x > 0).DefaultIfEmpty(0).Max();
            double maxNegativeLagrange = values.LagrangeValues.Where(x => x < 0).DefaultIfEmpty(0).Min();
            double avgLagrange = values.LagrangeValues.Average();
            double maxAbsoluteLagrange = Math.Abs(maxPositiveLagrange) > Math.Abs(maxNegativeLagrange) ? maxPositiveLagrange : maxNegativeLagrange;

            if (correlation >= -1 && correlation < -0.5)
            {
                unreliableTM.Add(key, correlation);
            }
            else if (correlation >= -0.5 && correlation < 0.5)
            {
                questionableTM.Add(key, correlation);
            }
           //else
           //{
           //    reliableTM.Add(key, correlation);
           //}
        }

        static void DisplayTelemetryData(string header, Dictionary<TMKey, double> data)
        {
            Console.WriteLine(header);
            foreach (var item in data)
            {
                Console.WriteLine($"indexTM: {item.Key.Index}, id1: {item.Key.Id1}, id2: {item.Key.Id2}, id3: {item.Key.Id3}, Correlation: {Math.Round(item.Value, 3)}");
            }
        }
        static double CalculateCorrelation(List<double> measuredValues, List<double> estimatedValues)
        {
            int count = measuredValues.Count;
            if (count == 0) return double.NaN;

            double avgMeasured = measuredValues.Average();
            double avgEstimated = estimatedValues.Average();

            double covariance = 0;
            double varianceMeasured = 0;
            double varianceEstimated = 0;

            for (int i = 0; i < count; i++)
            {
                double measuredDiff = measuredValues[i] - avgMeasured;
                double estimatedDiff = estimatedValues[i] - avgEstimated;

                covariance += measuredDiff * estimatedDiff;
                varianceMeasured += measuredDiff * measuredDiff;
                varianceEstimated += estimatedDiff * estimatedDiff;
            }

            double denominator = Math.Sqrt(varianceMeasured * varianceEstimated);
            if (denominator == 0) return double.NaN;

            return covariance / denominator;
        }
        static string DetermineStatus(double correlation)
        {
            if (correlation >= -1 && correlation < -0.5) return "Недостоверная";
            if (correlation >= -0.5 && correlation < 0.5) return "Сомнительная";
            if (correlation >= 0.5 && correlation <= 1) return "Достоверная";
            return "Неопределено";
        }
    }

    public class TMKey
    {
        public double Index { get; }
        public int Id1 { get; }
        public int Id2 { get; }
        public int Id3 { get; }

        public TMKey(double index, int id1, int id2, int id3)
        {
            Index = index;
            Id1 = id1;
            Id2 = id2;
            Id3 = id3;
        }

        public override bool Equals(object obj)
        {
            if (obj is TMKey otherKey)
            {
                return Index == otherKey.Index && Id1 == otherKey.Id1 && Id2 == otherKey.Id2 && Id3 == otherKey.Id3;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Index, Id1, Id2, Id3);
        }
    }

    public class TwoList
    {
        public List<double> MeasuredValues { get; }
        public List<double> EstimatedValues { get; }
        public List<double> LagrangeValues { get; }
        public List<string> PrivyazkaTM { get; }
        public List<double> DeltaIzmOc { get; }
        public List<string> Names { get; }
        public List<string> Srez { get; }
        public List<Guid> SliceIDs { get; }

        public TwoList(List<double> measuredValues, List<double> estimatedValues, List<double> lagrangeValues, List<string> privyazkaTM, List<double> deltaIzmOc, List<string> names, List<string> srez)
        {
            MeasuredValues = measuredValues;
            EstimatedValues = estimatedValues;
            LagrangeValues = lagrangeValues;
            PrivyazkaTM = privyazkaTM;
            DeltaIzmOc = deltaIzmOc;
            Names = names;
            Srez = srez;
            SliceIDs = new List<Guid>();
        }
    }
    public class TM
    {
        public double IndexTM { get; set; }
        public double Correlation { get; set; }
        public string Type { get; set; }
        public string Privyazka { get; set; }
        public double MaxLagrange { get; set; }
        public double AvgLagrange { get; set; }
        public string NameTM { get; set; }
    }

}
