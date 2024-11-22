using System;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace SeatingOptimizer
{
    
    public static class SeatCalc
    {
        public static List<HashSet<int>> GetClusters(List<SeatRequest> seatRequests, int tableLength)
        {
            AdjacencySetGraph graph = new AdjacencySetGraph(seatRequests);
            var clusters = graph.FindClusters();

            int largestClusterSize = 0;
            foreach (var cluster in clusters)
            {
                if(cluster.Count > largestClusterSize)
                    largestClusterSize = cluster.Count;
            }

            if(largestClusterSize > tableLength * 2)
                throw new Exception("Table length is too small, largest found clique contains " + largestClusterSize + " people");

            return clusters;
        }
    }

    public class Optimizer
    {
        static Random random = new Random();
        private int tableLength;
        private int[] seats;
        private List<HashSet<int>> clusters;
        private int bestScore;
        private int[] bestSeats;
        private List<SeatRequest> seatRequests;
        private int numberOfTables;

        // Scoring weights
        private const int BESIDE_SCORE = 4;
        private const int ACROSS_SCORE = 3;
        private const int DIAGONAL_SCORE = 2;
        private const int NOTALONE_SCORE = 1;

        public Optimizer(List<HashSet<int>> clusters, int tableLength, List<SeatRequest> seatRequests)
        {
            this.clusters = clusters;
            this.tableLength = tableLength;
            this.seatRequests = seatRequests;

            int seatsPerTable = tableLength * 2;
            this.numberOfTables = (int)Math.Ceiling((double)seatRequests.Count / seatsPerTable);

            // Total seats array now accounts for all tables
            this.seats = new int[seatsPerTable * numberOfTables];
            this.bestSeats = new int[seatsPerTable * numberOfTables];
        }
        private int CalculateScore(int[] arrangement)
        {
            int score = 0;
            int seatsPerTable = tableLength * 2;

            for (int tableIndex = 0; tableIndex < numberOfTables; tableIndex++)
            {
                int tableOffset = tableIndex * seatsPerTable;

                // Score each position in current table
                for (int i = 0; i < seatsPerTable; i++)
                {
                    int absolutePosition = tableOffset + i;
                    var person = arrangement[absolutePosition];
                    var request = seatRequests.FirstOrDefault(r => r.Id == person);
                    if (request == null) continue;

                    // Check beside (left and right) - only within same side of table
                    if (i % tableLength > 0) // Not leftmost seat
                        score += CheckAdjacency(arrangement[absolutePosition - 1], request, BESIDE_SCORE);
                    if (i % tableLength < tableLength - 1) // Not rightmost seat
                        score += CheckAdjacency(arrangement[absolutePosition + 1], request, BESIDE_SCORE);

                    // Check across - within same table
                    int oppositeIndex = tableOffset + ((i + tableLength) % seatsPerTable);
                    score += CheckAdjacency(arrangement[oppositeIndex], request, ACROSS_SCORE);

                    // Check diagonals - within same table
                    if (i % tableLength > 0) // Not leftmost seat
                        score += CheckAdjacency(arrangement[oppositeIndex - 1], request, DIAGONAL_SCORE);
                    if (i % tableLength < tableLength - 1) // Not rightmost seat
                        score += CheckAdjacency(arrangement[oppositeIndex + 1], request, DIAGONAL_SCORE);
                }
            }
            return score;
        }

        private int CheckAdjacency(int adjacentPerson, SeatRequest request, int scoreValue)
        {
            return request.RequestedAdjacentsId.Contains(adjacentPerson) ? scoreValue : 0;
        }

        public int[] Optimize(double initialTemperature = 100.0, double coolingRate = 0.995, int iterations = 10000)
        {
            // Initialize random arrangement
            InitializeRandomArrangement();

            bestScore = CalculateScore(seats);
            Array.Copy(seats, bestSeats, seats.Length);

            double temperature = initialTemperature;

            while (temperature > 0.1 && iterations-- > 0)
            {
                int[] newArrangement = (int[])seats.Clone();

                // Generate new solution by swapping two random positions within the same cluster
                var cluster = clusters[random.Next(clusters.Count)];
                SwapTwoSeatsInCluster(newArrangement, cluster);

                int newScore = CalculateScore(newArrangement);
                int scoreDelta = newScore - CalculateScore(seats);

                if (scoreDelta > 0 || Math.Exp(scoreDelta / temperature) > random.NextDouble())
                {
                    Array.Copy(newArrangement, seats, seats.Length);

                    if (newScore > bestScore)
                    {
                        bestScore = newScore;
                        Array.Copy(seats, bestSeats, seats.Length);
                    }
                }

                temperature *= coolingRate;
            }

            return bestSeats;
        }

        private void InitializeRandomArrangement()
        {
            // Initialize all seats as empty
            Array.Fill(seats, -1);
            int seatsPerTable = tableLength * 2;
            
            // Try to keep clusters together at the same table when possible
            foreach (var cluster in clusters)
            {
                var clusterArray = cluster.ToArray();
                int placed = 0;

                // Always points to the first empty seat table-wise, not array-wise
                int seatIndexPointer = -1;

                // Try to find a table with enough space for the whole cluster
                bool clusterPlaced = false;
                int tableOffset;
                if (clusterArray.Length <= tableLength * 2)  // Only try if cluster can fit at one table
                {
                    for (int tableIndex = 0; tableIndex < numberOfTables && !clusterPlaced; tableIndex++)
                    {
                        tableOffset = tableIndex * (tableLength * 2);
                        int emptySeatsAtTable = CountEmptySeats(seats, tableOffset, tableLength * 2);

                        if (emptySeatsAtTable >= clusterArray.Length)
                        {

                            seatIndexPointer = SeekFirstEmptySeatInOrder(tableOffset, seatIndexPointer);

                            // Place entire cluster at this table
                            while (placed < clusterArray.Length)
                            {
                                if (seats[seatIndexPointer] == -1)
                                {
                                    seats[seatIndexPointer] = clusterArray[placed];
                                    placed++;
                                }
                                // Go across the table and if on the bottom, go one seat to the right as well
                                if (seatIndexPointer < tableOffset + tableLength)
                                    seatIndexPointer = (seatIndexPointer + tableLength) % seatsPerTable;
                                else
                                    seatIndexPointer = (seatIndexPointer + tableLength) % seatsPerTable + 1;
                            }
                            clusterPlaced = true;
                        }
                    }
                }
                tableOffset = 0;
                seatIndexPointer = 0;
                // If couldn't place cluster together, place remaining members to next empty seats across all tables
                while (placed < clusterArray.Length && seatIndexPointer < seats.Length)
                {
                    
                    if (seats[seatIndexPointer] == -1)
                    {
                        seats[seatIndexPointer] = clusterArray[placed];
                        placed++;
                    }

                    // Next table if pointer has reached the last seat
                    if (seatIndexPointer == tableOffset + tableLength * 2 - 1)
                    {
                        tableOffset++;
                        seatIndexPointer++;
                    }
                    else
                    {
                        // Go across the table and if on the bottom, go one seat to the right as well
                        if (seatIndexPointer < tableOffset + tableLength)
                            seatIndexPointer = (seatIndexPointer + tableLength) % seatsPerTable;
                        else
                            seatIndexPointer = (seatIndexPointer + tableLength) % seatsPerTable + 1;
                    }
                   
                }
            }

            int SeekFirstEmptySeatInOrder(int tableOffset, int seatIndexPointer)
            {
                // Seek first empty seat of given table
                for (int i = tableOffset; i < tableOffset + tableLength * 2;)
                {
                    if (seats[i] == -1)
                    {
                        seatIndexPointer = i;
                        break;
                    }
                    // Go across the table and if on the bottom, go one seat to the right
                    if (i < tableOffset + tableLength)
                        i = tableOffset + (i + tableLength) % seatsPerTable;
                    else
                        i = tableOffset + (i + tableLength) % seatsPerTable + 1;
                }

                return seatIndexPointer;
            }
        }

        private int CountEmptySeats(int[] arrangement, int startIndex, int length)
        {
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                if (arrangement[startIndex + i] == -1)
                    count++;
            }
            return count;
        }

        private void SwapTwoSeatsInCluster(int[] arrangement, HashSet<int> cluster)
        {
            int pos1, pos2;

            if (cluster.Count < 2)
            {
                return;
            }

            // First position can be anywhere
            do
            {
                pos1 = random.Next(arrangement.Length);
            } while (!cluster.Contains(arrangement[pos1]));

            // For smaller clusters, try to keep them at the same table
            if (cluster.Count <= tableLength * 2)
            {
                // Get the table index of the first position
                int tableIndex = pos1 / (tableLength * 2);
                int tableStart = tableIndex * (tableLength * 2);
                int tableEnd = tableStart + (tableLength * 2);

                // Try several times to find a swap position at the same table
                int attempts = 5;
                while (attempts-- > 0)
                {
                    pos2 = random.Next(tableStart, tableEnd);
                    if (pos1 != pos2 && cluster.Contains(arrangement[pos2]))
                    {
                        // Swap within same table
                        (arrangement[pos1], arrangement[pos2]) = (arrangement[pos2], arrangement[pos1]);
                        return;
                    }
                }
            }

            // If same-table swap not possible or not required, swap with any valid position
            do
            {
                pos2 = random.Next(arrangement.Length);
            } while (pos1 == pos2 || !cluster.Contains(arrangement[pos2]));

            (arrangement[pos1], arrangement[pos2]) = (arrangement[pos2], arrangement[pos1]);
        }
    }

    public static class CsvParser
    {
        // Return a list of seat requests that represent each person and the people they requested to be adjacent to
        public static List<SeatRequest> ParseCsv(string csvData, string nameColumnName, string requestedAdjacentPeopleColumnName)
        {
            List<SeatRequest> seatRequests = new List<SeatRequest>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                Quote = '"',
                Encoding = Encoding.UTF8

            };

            using var reader = new StringReader(csvData);
            using var csv = new CsvReader(reader, config);
            {
                SeatRequestMap seatRequestMap = new SeatRequestMap(nameColumnName, requestedAdjacentPeopleColumnName);
                csv.Context.RegisterClassMap(seatRequestMap);
                try
                {
                    var records = csv.GetRecords<SeatRequest>();
                    seatRequests = records.ToList();
                }
                catch (BadDataException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }

            // Give all seat requests an ID
            for (int i = 0; i < seatRequests.Count; i++)
            {
                seatRequests[i].Id = i;
            }
            var nameToIdDictionary = seatRequests.ToDictionary(SeatRequest => SeatRequest.Name, SeatRequest => SeatRequest.Id);

            // Populate the ID version of the list of requested adjacents for everyone
            for (int i = 0; i < seatRequests.Count; i++)
            {
                foreach (string adjacentPerson in seatRequests[i].RequestedAdjacentsString)
                {
                    seatRequests[i].RequestedAdjacentsId.Add(nameToIdDictionary[adjacentPerson]);
                }
            }   

            return seatRequests;
        }
    }
}
