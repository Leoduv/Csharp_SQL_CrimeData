using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Diagnostics;

namespace projetBDD
{
    class Program
    {   
        static void Main(string[] args)
        {
            
            bool fin = false;
            bool valid = true;
            string lecture = "";

            string connectionString = "SERVER=localhost;PORT=3306;DATABASE=NY_Crimes;UID=esilvs6;PASSWORD=esilvs6;";
            MySqlConnection connection = new MySqlConnection(connectionString);

            //Menu interactif
            //---------------
            do
            {
                fin = false;
                //
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("1 : Importer une journée de crimes");
                Console.WriteLine("2 : Exporter le bilan journalier");
                Console.WriteLine("3 : Saisie d'un crime");
                Console.WriteLine("4 : Le nombre de crimes par quartier et par catégorie");
                Console.WriteLine("5 : Récapitulatif pour un mois");
                Console.WriteLine("6 : Evolution mois par mois");
                Console.WriteLine("7 : Palmarès annuel");
                Console.WriteLine("8 : Innovation : nombre moyen de crimes par quartiers");
                Console.WriteLine("9 : fin");
                //
                do
                {
                    lecture = "";
                    valid = true;

                    Console.Write("\nChoisissez un programme > ");
                    lecture = Console.ReadLine();
                    Console.WriteLine(lecture);
                    if (lecture == "" || !"123456789".Contains(lecture[0]))
                    {
                        Console.WriteLine("votre choix <" + lecture + "> n'est pas valide = > recommencez ");
                        valid = false;
                    }
                } while (!valid);
                //
                //
                switch (lecture[0])
                {
                    case '1'://OK
                        Console.Clear();
                        InsertionJournee(connection);
                        break;
                    case '2'://OK
                        Console.Clear();
                        ExporterBilanJournalier(connection);
                        break;
                    case '3':
                        Console.Clear();
                        SaisirUnCrime(connection);
                        break;
                    case '4':
                        Console.Clear();
                        CrimesParQuartier(connection);
                        break;
                    case '5':
                        Console.Clear();
                        RecapitulatifMensuel(connection);
                        break;
                    case '6':
                        Console.Clear();
                        EvolutionMoisParMois(connection);
                        break;
                    case '7':
                        Console.Clear();
                        PalmaresAnnuel(connection);
                        break;
                    case '8':
                        Console.Clear();
                        NbrMoyenCrimesQuartiers(connection);
                        break;
                    case '9':
                        Console.Clear();
                        Console.WriteLine("fin de programme...");
                        Console.ReadKey();
                        fin = true;
                        break;
                    default:
                        Console.WriteLine("\nchoix non valide => faites un autre choix....");
                        break;
                }
            } while (!fin);
        }

        /// <Importation_crime_XML>
        /// Extrait l'information contenue dan sla xml et la stock dans une list de string
        /// Marque le debute t la fin de chaque crime dans la list
        /// Pour chaque crime dans la list : 
        ///     -utilise deux requetes pour obtenir le crime_id et jurisdiction_id en fcontion des infos fournies
        ///     -cree une commande d insertion dans la database et l execute
        /// </summary>//OK
        static void InsertionJournee(MySqlConnection connection)
        {
            Console.WriteLine("\n1 => Importer une journée de crimes (question 1.1)");
            Console.WriteLine("-----------------\n");
            Console.WriteLine("Entrez le nom du fichier");

            string nomfichier = Console.ReadLine();

            XmlTextReader reader = new XmlTextReader(nomfichier);
            //Recuperation des donnees contenues dans le XML
            List<string> crimes = new List<string>();//list qui va contenir toutes les infps du xml
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: //Ce noeud est un element
                        if (reader.Name == "crime")
                        {
                            crimes.Add("DEBUT");//marque le debut de la description du crime dans la liste de string
                        }
                        break;
                    case XmlNodeType.Text: //Concerne le texte etre les deux <> et <\>
                        crimes.Add(reader.Value);//ajoute le texte dans la liste
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.*
                        //Console.WriteLine(reader.Name);
                        if(reader.Name == "crime")
                        {
                            crimes.Add("FIN");//marque la fin de la description
                        }
                        break;
                }
            }
            //Affiche le contenu de la list de string
            Console.WriteLine("Crimes : ");
            foreach (string elem in crimes)
            {
                if (elem == "DEBUT")
                {
                    Console.WriteLine();
                }
                Console.Write(elem + ";");
            }
            Console.ReadLine();

            //Detection du debut de la description de chaque crime dans la list de strings
            List<int> index_debut_crimes = new List<int>();
            int index = 0;
            foreach (string elem in crimes)
            {
                if (elem == "DEBUT")//Repere le debut d'une description de crime
                {
                    index_debut_crimes.Add(index);
                }
                index += 1;
            }

            //Creation elements de la commande en utilisation la list d index
            string date = crimes[index_debut_crimes[0] + 2] + "/" + crimes[index_debut_crimes[0] + 1] + "/2012"; //meme date pour les crimes
            string borough;
            string crime_description;
            string specificity;
            int crime__description_id = 0;
            string jurisdiction;
            int jurisdiction_id = 0;
            //on fixe arbitrairement les coordonnées
            int coord_X = 998578;
            int coord_Y = 755684;
            foreach (int debut in index_debut_crimes)
            {
                //creation des elements de la commande
                borough = crimes[debut + 3];
                crime_description = crimes[debut + 4];
                specificity = crimes[debut + 5];
                jurisdiction = crimes[debut + 6];

                //on utilise un erequete pour obtenir le numero de jurisdiction
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = " SELECT id "
                                        + "FROM Jurisdiction "
                                        + "WHERE Jurisdiction.name = '" + jurisdiction + "';";
                MySqlDataReader lecteur;
                lecteur = command.ExecuteReader();
                while (lecteur.Read())
                {
                    jurisdiction_id = lecteur.GetInt32(0);
                }
                connection.Close();

                //on utilise une requete pour obtenir le numero de crime
                connection.Open();
                command.CommandText = " SELECT id "
                                        + "FROM Crime_description "
                                        + "WHERE Crime_description.desc_specificity = '" + specificity + "';";
                lecteur = command.ExecuteReader();
                while(lecteur.Read())
                {
                    crime__description_id = lecteur.GetInt32(0);
                }
                connection.Close();


                //COMMANDE FINALE
                connection.Open();
                string values = date + "', '" + borough + "', '" + coord_X + "', '" + coord_Y
                                    + "', '" + crime__description_id + "', '" + jurisdiction_id + "');";
                command.CommandText = " INSERT INTO NY_Crimes.Crime (date,borough,coord_X,coord_Y,crime_description_id,jurisdiction_id) "
                                    + " VALUES('" + values;
                command.ExecuteNonQuery();
                connection.Close();
            }
            Console.WriteLine("Crimes du " + date + " ajoutes avec succes");
            Console.WriteLine("-----------------\n\n");
            Console.ReadKey();
        }

        /// <Exportation_requete_XML>
        /// Demande a l utilisateur une date et creer un fichier xml contenant les crimes de la journee
        /// Demmande la date a l utilisateur
        /// Stock la commande associee dans un string
        /// Ouvre un dataset et applique la commande voule et le rempli avec les resultats 
        /// Cree un fichier XML et le rempli avec le cotnenu du dataset
        /// Ouvre le fichier
        /// </summary>
        static void ExporterBilanJournalier(MySqlConnection connection)
        {
            Console.WriteLine("\n2 => Exporter le bilan journalier (question 1.2)");
            Console.WriteLine("-----------------\n");
            Console.Write("Saisir la date voulue \nNumero du mois (01 jusq'a 12) > ");
            string mois = Console.ReadLine();
            Console.Write("Numero du jour (01 jusq'a 30 ou 31 sauf Fevrier) > ");
            string jour = Console.ReadLine();
            //on fait deux versions de la date
            string date_commande = mois + "/" + jour + "/2012"; //version respectant grammaire commande
            string date_fichier = mois + "-" + jour; //version sans / pour le nom du fichier

            //creation d un string contenant la commande a effectuer
            string commande = "SELECT date, borough, CD.description as desc_crime, CD.desc_specificity, J.name as jurisdiction " +
                "FROM Crime as C, Crime_description as CD, Jurisdiction as J " +
                "WHERE C.date = '" + date_commande + "' AND C.crime_description_id = CD.id AND C.jurisdiction_id = J.id;";

            connection.Open();

            DataSet mon_dataset = new DataSet("resultats");
            MySqlDataAdapter myDa = new MySqlDataAdapter();

            //applique la commande cree dans le dataset avec la connection ouverte
            myDa.SelectCommand = new MySqlCommand(commande, connection);
            myDa.Fill(mon_dataset, "Crime"); //rempli le dataset du resultat de la commande
            //creation du fichier
            string nom_fichier = "Rapport_Journalier-" + date_fichier;
            FileStream stream = new FileStream(nom_fichier, FileMode.OpenOrCreate, FileAccess.Write);
            mon_dataset.WriteXml(stream);
            stream.Close();
            //ouverture du fichier
            Process.Start(nom_fichier);

            Console.WriteLine("-----------------\n\n");
            connection.Close();
            Console.ReadKey();
        }

        /// <Ajoute un crime a la base de donnees>
        /// Demande des informations sur le crime a l utilisateur
        /// Cree une requete sql avec ces infos et l'execute
        /// </summary>
        static void SaisirUnCrime(MySqlConnection connection)
        {
            Console.WriteLine("\n3 => Saisie d'un crime (question 2.1)");
            Console.WriteLine("-----------------\n");
            Console.Write("Saisissez la date du crime, par exemple 03/21 > ");
            string date = Console.ReadLine() + "/2012";
            Console.Write("Quartier du crime > ");
            string quartier = Console.ReadLine();
            Console.Write("ID du type de crime > ");
            string crime_id = Console.ReadLine();
            Console.Write("ID de la jurisdiction > ");
            string jurisdiction_id = Console.ReadLine();

            //creation de la commande
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO NY_Crimes.Crime(date,borough,coord_X,coord_Y,crime_description_id,jurisdiction_id) " +
                "VALUES('" + date + "', '" + quartier + "', " + "1025089, 181315, " + crime_id + ", " + jurisdiction_id + ");";

            //execution de la commande
            connection.Open();
            command.ExecuteNonQuery();
            Console.WriteLine("Crime ajouté");
            connection.Close();

            Console.WriteLine("-----------------\n\n");
            Console.ReadKey();
        }

        /// <Affiche le nombre de crime par quartier et par type pour une date donnee>
        /// Demande a l'utilisateur la date souhaitee
        /// Cree une commande SQL avec cette date 
        /// Execute la requete et affiche les resultats
        /// </summary>
        static void CrimesParQuartier(MySqlConnection connection)
        {
            Console.WriteLine("\n4 => Le nombre de crimes par quartier et par catégorie (question 3.1)");
            Console.WriteLine("-----------------\n");
            Console.Write("Saisissez la date du crime, par exemple 03/21 > ");
            string date = Console.ReadLine() + "/2012";

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "select borough, cd.description, count(*) " +
                                   " from crime as c, crime_description as cd" +
                                   " where c.date = '" + date + "' " +
                                   " and c.crime_description_id = cd.id" +
                                   " group by borough, cd.description" +
                                   " order by borough, cd.description;";

            connection.Open();
            MySqlDataReader reader;
            reader = command.ExecuteReader();
            string[] valueString = new string[reader.FieldCount];
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    //affichage
                    valueString[i] = reader.GetValue(i).ToString();
                    Console.Write(valueString[i] + " | ");
                }
                Console.WriteLine();
            }
            connection.Close();

            Console.WriteLine("-----------------\n\n");
            Console.ReadKey();
        }

        /// <Affiche le nombre de crimes par quartier pour un mois>
        /// Demande a l'utilisateur le mois voulu
        /// Creer et execute une requete avec le mois selectionné
        /// Affiche les resultats
        /// </summary>
        static void RecapitulatifMensuel(MySqlConnection connection)
        {
            Console.WriteLine("\n5 => Récapitulatif pour un mois (question 3.2)");
            Console.WriteLine("-----------------\n");
            Console.Write("Saisissez le mois voulu ex:04 > ");
            string mois = Console.ReadLine();

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "select borough, count(*) description " +
                                   " from crime as c, crime_description as cd" +
                                   " where cd.description = 'grand larceny' and c.date like '" + mois + "%'" +
                                   " and c.crime_description_id = cd.id" +
                                   " group by borough;";

            connection.Open();
            MySqlDataReader reader;
            reader = command.ExecuteReader();
            string[] valueString = new string[reader.FieldCount];
            Console.WriteLine("Nombre de crimes par quartiers : ");
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    //affichage
                    valueString[i] = reader.GetValue(i).ToString();
                    Console.Write(valueString[i] + " | ");
                }
                Console.WriteLine();
            }
            connection.Close();

            Console.WriteLine("-----------------\n\n");
            Console.ReadKey();
        }

        static void AffichagePourcentages(double[,] pourcentages)
        {
            Console.WriteLine("Evolution mois par mois en pourcentage du nombre de Grand Larcery par quartier\n");
            Console.WriteLine("      | Jan | Fev | Mar | Avr | Mai | Jun | Jui | Aou | Sep | Oct | Nov | Dec |");
            string[] quartiers = { "BRON", "BROO", "MANH", "QUEE", "STAT" };
            for(int i = 0; i < pourcentages.GetLength(0); i++)
            {
                Console.Write(" " + quartiers[i] + " | ");
                for(int j = 0; j < pourcentages.GetLength(1); j++)
                {
                    if (pourcentages[i, j] < 10)
                    {
                        Console.Write(" " + pourcentages[i, j] + "% | ");
                    }
                    else { Console.Write(pourcentages[i, j] + "% | "); }
                }
                Console.WriteLine();
            }
        }
        static double CalculPourcentage(double quartier, double total)
        {
            double result = (quartier / total) * 100;
            result = Math.Round(result);
            return result;
        }
        /// <Affiche l'evolution de % Grand Larceny par quartier>
        /// Trouve le nombre totaux de crimes par mois (requete)
        ///     Les stock dans un tableau
        ///  Trouve le nombre de Grand Larceny par mois
        ///     Stock les resultats dans une matrice
        ///  Calcul le pourcentage en arrondissant dans chaque case de la matrice a l'aide de la fonction CalculPourcentage
        ///  Affiche le resultat final en arrondissant, fonction AffichagePourcentages
        /// </summary>
        static void EvolutionMoisParMois(MySqlConnection connection)
        {
            Console.WriteLine("\n6 => Evolution mois par mois (question 3.2)");
            Console.WriteLine("-----------------\n");
            double[,] pourcentages = new double[5, 12];
            for (int i = 0; i < pourcentages.GetLength(0); i++)
            {
                for (int j = 0; j < pourcentages.GetLength(1); j++)
                {
                    pourcentages[i, j] = Math.Round(2.6);
                }
            }


            //Tableau des nombre totaux de crimes par mois
            double[] nbr_tot = new double[12];
            string mois = "";
            for (int i = 1; i <= 12; i++)
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                if(i<10)
                {
                    mois = "0" + Convert.ToString(i);
                }
                else { mois = Convert.ToString(i); }
                command.CommandText = "select count(*) " +
                                      "from Crime " +
                                      "where date like '" + mois + "%';";
                MySqlDataReader reader;
                reader = command.ExecuteReader();
                reader.Read();
                nbr_tot[i-1] = reader.GetDouble(0);
                connection.Close();
            }

            //List des nombre de grand larcery par quartier
            double[,] nbr_quartiers = new double[5, 12];
            for(int i = 1; i <= 12; i++)
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                if (i < 10)
                {
                    mois = "0" + Convert.ToString(i);
                }
                else { mois = Convert.ToString(i); }
                command.CommandText = "select count(*), borough " +
                                      "from crime as c, crime_description as cd " +
                                      "where cd.description = 'grand larceny' and c.date like '" + mois +
                                      "%' and c.crime_description_id = cd.id " +
                                      "group by borough order by borough;";
                //trier par ordre alphabetique permet de savoir quel nbr pour quel quartier

                MySqlDataReader reader;
                reader = command.ExecuteReader();
                int ligne = 0;
                while(reader.Read())
                {
                    nbr_quartiers[ligne, i-1] = reader.GetDouble(0);
                    ligne++;
                }
                connection.Close();
            }         
            for(int i = 0; i < 5; i++)
            {
                for(int j = 0; j < 12; j++)
                {
                    pourcentages[i, j] = CalculPourcentage(nbr_quartiers[i, j], nbr_tot[j]);
                }
            }
            AffichagePourcentages(pourcentages);
            Console.WriteLine("-----------------\n\n");
            Console.ReadKey();
        }

        /// <Affiche le palmares annuel du pire quartiers>
        /// Repere avec une requete SQL le pire quartier (celui avec le plus de crimes)
        /// Repere ensuite le nombre de crimes par categorie dans ce quartier
        /// Affiche le resultat
        /// </summary>
        static void PalmaresAnnuel(MySqlConnection connection)
        {
            Console.WriteLine("\n7 => Palmarès annuel (question 4.1)");
            Console.WriteLine("-----------------\n");

            //On repere le quartier ayant le plus de crime dans l'annee
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "select max(nbr), borough " +
                                  "from(select count(*) as nbr, borough " +
                                        "from crime " +
                                        "group by borough) as temp; ";

            MySqlDataReader reader;
            reader = command.ExecuteReader();
            int nbr_max = 0;
            string pire_quartier = "";
            while(reader.Read())
            nbr_max = reader.GetInt32(0); //nombre rangés dans la 1ère colonne
            pire_quartier = reader.GetString(1); //nom du quartier associe dans la 2eme colonne

            connection.Close();
            Console.WriteLine("Le pire quartier de 2012 est " + pire_quartier + " avec " + nbr_max + " crimes en tout.");

            //On regarde le nombre de crimes par type dans ce quartier
            connection.Open();
            command.CommandText = "select count(*), description " +
                                  "from crime c, crime_description cd " +
                                  "where c.crime_description_id = cd.id and c.borough = '" + pire_quartier + "' group by description;";

            reader = command.ExecuteReader();
            while(reader.Read())
            {
                Console.Write("Nombre de " + reader.GetString(1));
                Console.WriteLine(" : " + reader.GetInt32(0));
            }
            connection.Close();
            Console.WriteLine("-----------------\n\n");
            Console.ReadKey();
        }

        /// <Affiche le nombre moyen de crime par mois dans chaque quartier>
        /// Avec une seule requete, compte le nombre de crime dans chaque quartier et calcul la moyenne
        /// Affiche le resultat
        /// </summary>
        static void NbrMoyenCrimesQuartiers(MySqlConnection connection)
        {
            Console.WriteLine("\n8La fonction de mon choix (question 5.1) :\n " +
                "Nombre moyen de crime par mois par quartiers");
            Console.WriteLine("-----------------\n");

            //Requete : nombre de crimes par quartier et calcul du nombre moyen
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "select count(*)/12 as nbr, borough" +
                                    " from crime as c" +
                                    " group by borough order by nbr desc;";

            MySqlDataReader reader;
            reader = command.ExecuteReader();

            //recuperation des donnees et affichage des resultats
            while(reader.Read())
            {
                Console.WriteLine(reader.GetString(1) + " : " + Math.Round(reader.GetDouble(0), 1)); //Round pour arrondir
            }
            connection.Close();
            Console.WriteLine("-----------------\n\n");
            Console.ReadKey();
        }
    }
}
