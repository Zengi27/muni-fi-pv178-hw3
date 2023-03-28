using PV178.Homeworks.HW03.DataLoading.DataContext;
using PV178.Homeworks.HW03.DataLoading.Factory;
using PV178.Homeworks.HW03.Model;
using PV178.Homeworks.HW03.Model.Enums;

namespace PV178.Homeworks.HW03
{
    public class Queries
    {
        private IDataContext? _dataContext;
        public IDataContext DataContext => _dataContext ??= new DataContextFactory().CreateDataContext();

        /// <summary>
        /// Ukážkové query na pripomenutie základnej LINQ syntaxe a operátorov. Výsledky nie je nutné vracať
        /// pomocou jedného príkazu, pri zložitejších queries je vhodné si vytvoriť pomocné premenné cez `var`.
        /// Toto query nie je súčasťou hodnotenia.
        /// </summary>
        /// <returns>The query result</returns>
        public int SampleQuery()
        {
            return DataContext.Countries
                .Where(a => a.Name?[0] >= 'A' && a.Name?[0] <= 'G')
                .Join(DataContext.SharkAttacks,
                    country => country.Id,
                    attack => attack.CountryId,
                    (country, attack) => new { country, attack }
                )
                .Join(DataContext.AttackedPeople,
                    ca => ca.attack.AttackedPersonId,
                    person => person.Id,
                    (ca, person) => new { ca, person }
                )
                .Where(p => p.person.Sex == Sex.Male)
                .Count(a => a.person.Age >= 15 && a.person.Age <= 40);
        }

        /// <summary>
        /// Úloha č. 1
        ///
        /// Vráťte zoznam, v ktorom je textová informácia o každom človeku,
        /// na ktorého v štáte Bahamas zaútočil žralok s latinským názvom začínajúcim sa 
        /// na písmeno I alebo N.
        /// 
        /// Túto informáciu uveďte v tvare:
        /// "{meno človeka} was attacked in Bahamas by {latinský názov žraloka}"
        /// </summary>
        /// <returns>The query result</returns>
        public List<string> InfoAboutPeopleThatNamesStartsWithCAndWasInBahamasQuery()
        {
            var attackedPeople = DataContext.Countries
                .Where(c => c.Name == "Bahamas")
                .Join(DataContext.SharkAttacks,
                    c => c.Id,
                    sa => sa.CountryId,
                    (c, sa) => new { c, sa })
                .Join(DataContext.SharkSpecies,
                    csa => csa.sa.SharkSpeciesId,
                    sharkSpecies => sharkSpecies.Id,
                    (csa, sharkSpecies) => new { csa.c, csa.sa, sharkSpecies})
                .Where(ss => ss.sharkSpecies.LatinName.StartsWith("I") || ss.sharkSpecies.LatinName.StartsWith("N"))
                .Join(DataContext.AttackedPeople,
                    csass => csass.sa.AttackedPersonId,
                    person => person.Id,
                    (csass, person) => new {csass.sharkSpecies, person})
                .Select(ssp => $"{ssp.person.Name} was attacked in Bahamas by {ssp.sharkSpecies.LatinName}")
                .ToList();

            foreach (var array in attackedPeople.ToList()) 
                Console.WriteLine(string.Join(" ", array));
            
            return attackedPeople;
        }

        /// <summary>
        /// Úloha č. 2
        ///
        /// Firma by chcela expandovať do krajín s nedemokratickou formou vlády – monarchie alebo teritória.
        /// Pre účely propagačnej kampane by chcela ukázať, že žraloky v týchto krajinách na ľudí neútočia
        /// s úmyslom zabiť, ale chcú sa s nimi iba hrať.
        /// 
        /// Vráťte súhrnný počet žraločích utokov, pri ktorých nebolo preukázané, že skončili fatálne.
        /// 
        /// Požadovany súčet vykonajte iba pre krajiny s vládnou formou typu 'Monarchy' alebo 'Territory'.
        /// </summary>
        /// <returns>The query result</returns>
        public int FortunateSharkAttacksSumWithinMonarchyOrTerritoryQuery()
        {
            var nonDemocracy = DataContext.Countries
                .Where(c => c.GovernmentForm == GovernmentForm.Monarchy ||
                            c.GovernmentForm == GovernmentForm.Territory);

            var nonFatalSeverenity = DataContext.SharkAttacks
                .Where(a => a.AttackSeverenity != AttackSeverenity.Fatal);

            var sum = nonDemocracy
                .Join(nonFatalSeverenity,
                    country => country.Id,
                    attack => attack.CountryId,
                    (country, attack) => new { country, attack })
                .Count();

            return sum;
        }

        /// <summary>
        /// Úloha č. 3
        ///
        /// Marketingovému oddeleniu dochádzajú nápady ako pomenovávať nové produkty.
        /// 
        /// Inšpirovať sa chcú prezývkami žralokov, ktorí majú na svedomí najviac
        /// útokov v krajinách na kontinente 'South America'. Pre pochopenie potrebujú 
        /// tieto informácie vo formáte slovníku:
        /// 
        /// (názov krajiny) -> (prezývka žraloka s najviac útokmi v danej krajine)
        /// </summary>
        /// <returns>The query result</returns>
        public Dictionary<string, string> MostProlificNicknamesInCountriesQuery()
        {
            // TODO...
            throw new NotImplementedException();
        }

        /// <summary>
        /// Úloha č. 4
        ///
        /// Firma chce začať kompenzačnú kampaň a potrebuje k tomu dáta.
        ///
        /// Preto zistite, ktoré žraloky útočia najviac na mužov. 
        /// Vráťte ID prvých troch žralokov, zoradených zostupne podľa počtu útokov na mužoch.
        /// </summary>
        /// <returns>The query result</returns>
        public List<int> ThreeSharksOrderedByNumberOfAttacksOnMenQuery()
        {
            // TODO...
            throw new NotImplementedException();
        }

        /// <summary>
        /// Úloha č. 5
        ///
        /// Oslovila nás medzinárodná plavecká organizácia. Chce svojich plavcov motivovať možnosťou
        /// úteku pred útokom žraloka.
        ///
        /// Potrebuje preto informácie o priemernej rýchlosti žralokov, ktorí
        /// útočili na plávajúcich ľudí (informácie o aktivite počas útoku obsahuje "Swimming" alebo "swimming").
        /// 
        /// Pozor, dáta požadajú oddeliť podľa jednotlivých kontinentov. Ignorujte útoky takých druhov žralokov,
        /// u ktorých nie je známa maximálná rýchlosť. Priemerné rýchlosti budú zaokrúhlené na dve desatinné miesta.
        /// </summary>
        /// <returns>The query result</returns>
        public Dictionary<string, double> SwimmerAttacksSharkAverageSpeedQuery()
        {
            // TODO...
            throw new NotImplementedException();
        }

        /// <summary>
        /// Úloha č. 6
        ///
        /// Zistite všetky nefatálne (AttackSeverenity.NonFatal) útoky spôsobené pri člnkovaní 
        /// (AttackType.Boating), ktoré mal na vine žralok s prezývkou "Zambesi shark".
        /// Do výsledku počítajte iba útoky z obdobia po 3. 3. 1960 (vrátane) a ľudí,
        /// ktorých meno začína na písmeno z intervalu <D, K> (tiež vrátane).
        /// 
        /// Výsledný zoznam mien zoraďte abecedne.
        /// </summary>
        /// <returns>The query result</returns>
        public List<string> NonFatalAttemptOfZambeziSharkOnPeopleBetweenDAndKQuery()
        {
            // TODO...
            throw new NotImplementedException();
        }

        /// <summary>
        /// Úloha č. 7
        ///
        /// Zistilo sa, že desať najľahších žralokov sa správalo veľmi podozrivo počas útokov v štátoch Južnej Ameriky.
        /// 
        /// Vráťte preto zoznam dvojíc, kde pre každý štát z Južnej Ameriky bude uvedený zoznam žralokov,
        /// ktorí v tom štáte útočili. V tomto zozname môžu figurovať len vyššie spomínaných desať najľahších žralokov.
        /// 
        /// Pokiaľ v nejakom štáte neútočil žiaden z najľahších žralokov, zoznam žralokov u takého štátu bude prázdny.
        /// </summary>
        /// <returns>The query result</returns>
        public List<Tuple<string, List<SharkSpecies>>> LightestSharksInSouthAmericaQuery()
        {
            // TODO...
            throw new NotImplementedException();
        }

        /// <summary>
        /// Úloha č. 8
        ///
        /// Napísať hocijaký LINQ dotaz musí byť pre Vás už triviálne. Riaditeľ firmy vás preto chce
        /// využiť na testovanie svojich šialených hypotéz.
        /// 
        /// Zistite, či každý žralok, ktorý má maximálnu rýchlosť aspoň 56 km/h zaútočil aspoň raz na
        /// človeka, ktorý mal viac ako 56 rokov. Výsledok reprezentujte ako pravdivostnú hodnotu.
        /// </summary>
        /// <returns>The query result</returns>
        public bool FiftySixMaxSpeedAndAgeQuery()
        {
            // TODO...
            throw new NotImplementedException();
        }

        /// <summary>
        /// Úloha č. 9
        ///
        /// Ohromili ste svojim výkonom natoľko, že si od Vás objednali rovno textové výpisy.
        /// Samozrejme, že sa to dá zvladnúť pomocou LINQ.
        /// 
        /// Chcú, aby ste pre všetky fatálne útoky v štátoch začínajúcich na písmeno 'B' alebo 'R' urobili výpis v podobe: 
        /// "{Meno obete} was attacked in {názov štátu} by {latinský názov žraloka}"
        /// 
        /// Záznam, u ktorého obeť nemá meno
        /// (údaj neexistuje, nejde o vlastné meno začínajúce na veľké písmeno, či údaj začína číslovkou)
        /// do výsledku nezaraďujte. Získané pole zoraďte abecedne a vraťte prvých 5 viet.
        /// </summary>
        /// <returns>The query result</returns>
        public List<string> InfoAboutPeopleAndCountriesOnBorRAndFatalAttacksQuery()
        {
            var fatalAttacks = DataContext.SharkAttacks
                .Where(a => a.AttackSeverenity == AttackSeverenity.Fatal)
                .Join(DataContext.SharkSpecies,
                    attack => attack.SharkSpeciesId,
                    species => species.Id,
                    ((attack, species) => new {attack, species}));

            var countries = DataContext.Countries
                .Where(c => c.Name.StartsWith('B') || c.Name.StartsWith('R'));

            var people = DataContext.AttackedPeople
                .Where(p => !string.IsNullOrEmpty(p.Name) 
                            && !char.IsDigit(p.Name[0]) 
                            && char.IsUpper(p.Name[0]));

            var result = fatalAttacks
                .Join(countries,
                    sa => sa.attack.CountryId,
                    country => country.Id,
                    (sa, country) => new { sa, country })
                .Join(people,
                    sac => sac.sa.attack.AttackedPersonId,
                    person => person.Id,
                    (sac, person) => new { Shark = sac.sa.species, Country = sac.country, Person = person })
                .Select(sacp => $"{sacp.Person.Name} was attacked in {sacp.Country.Name} by {sacp.Shark.LatinName}")
                .OrderBy(s => s)
                .Take(5)
                .ToList();

            return result;
        }

        /// <summary>
        /// Úloha č. 10
        ///
        /// Nedávno vyšiel zákon, že každá krajina Európy začínajúca na písmeno A až L (vrátane)
        /// musí zaplatiť pokutu 250 jednotiek svojej meny za každý žraločí útok na ich území.
        /// Pokiaľ bol tento útok smrteľný, musia dokonca zaplatiť 300 jednotiek. Ak sa nezachovali
        /// údaje o tom, či bol daný útok smrteľný alebo nie, nemusia platiť nič.
        /// Áno, tento zákon je spravodlivý...
        /// 
        /// Vráťte informácie o výške pokuty európskych krajín začínajúcich na A až L (vrátane).
        /// Tieto informácie zoraďte zostupne podľa počtu peňazí, ktoré musia tieto krajiny zaplatiť.
        /// Vo finále vráťte 5 záznamov s najvyššou hodnotou pokuty.
        /// 
        /// V nasledujúcej sekcii môžete vidieť príklad výstupu v prípade, keby na Slovensku boli 2 smrteľné útoky,
        /// v Česku jeden fatálny + jeden nefatálny a v Maďarsku žiadny:
        /// <code>
        /// Slovakia: 600 EUR
        /// Czech Republic: 550 CZK
        /// Hungary: 0 HUF
        /// </code>
        /// 
        /// </summary>
        /// <returns>The query result</returns>
        public List<string> InfoAboutFinesInEuropeQuery()
        {
            var europeanCountries = DataContext.Countries
                .Where(c => c.Continent == "Europe" && c.Name?[0] >= 'A' && c.Name?[0] <= 'L');

            var europeanAttacks = DataContext.SharkAttacks
                .Join(europeanCountries,
                    attack => attack.CountryId,
                    country => country.Id,
                    (attack, country) => new { Attack = attack, Country = country })
                .Where(a => a.Attack.AttackSeverenity != AttackSeverenity.Unknown);

            var fines = europeanAttacks
                .GroupBy(a => a.Country)
                .Select(group => new
                {
                    Country = group.Key,
                    Fine = group.Sum(a => a.Attack.AttackSeverenity == AttackSeverenity.NonFatal ? 250 : 300)
                })
                .OrderByDescending(cf => cf.Fine)
                .Select(cf => $"{cf.Country.Name}: {cf.Fine} {cf.Country.CurrencyCode}")
                .ToList();

            foreach (var array in fines) 
                Console.WriteLine(string.Join(" ", array));
            
            return fines;
        }

        /// <summary>
        /// Úloha č. 11
        ///
        /// Organizácia spojených žraločích národov výhlásila súťaž: 5 druhov žralokov, 
        /// ktoré sú najviac agresívne získa hodnotné ceny.
        /// 
        /// Nájdite 5 žraločích druhov, ktoré majú na svedomí najviac ľudských životov,
        /// druhy zoraďte podľa počtu obetí.
        ///
        /// Výsledok vráťte vo forme slovníku, kde
        /// kľúčom je meno žraločieho druhu a
        /// hodnotou je súhrnný počet obetí spôsobený daným druhom žraloka.
        /// </summary>
        /// <returns>The query result</returns>
        public Dictionary<string, int> FiveSharkNamesWithMostFatalitiesQuery()
        {
            var fatalAttack = DataContext.SharkAttacks
                .Where(a => a.AttackSeverenity == AttackSeverenity.Fatal);

            var result = fatalAttack
                .Join(DataContext.SharkSpecies,
                    attack => attack.SharkSpeciesId,
                    species => species.Id,
                    (attack, species) => new { Attack = attack, Species = species })
                .GroupBy(a => a.Species)
                .Select(group => new
                {
                    Species = group.Key,
                    AttackCount = group.Count()
                })
                .OrderByDescending(sa => sa.AttackCount)
                .Take(5)
                .ToDictionary(sa => sa.Species.Name!, sa => sa.AttackCount);

            foreach (var array in result.ToList()) 
                Console.WriteLine(string.Join(" ", array));

            return result;
        }

        /// <summary>
        /// Úloha č. 12
        ///
        /// Riaditeľ firmy chce si chce podmaňiť čo najviac krajín na svete. Chce preto zistiť,
        /// na aký druh vlády sa má zamerať, aby prebral čo najviac krajín.
        /// 
        /// Preto od Vás chce, aby ste mu pomohli zistiť, aké percentuálne zastúpenie majú jednotlivé typy vlád.
        /// Požaduje to ako jeden string:
        /// "{1. typ vlády}: {jej percentuálne zastúpenie}%, {2. typ vlády}: {jej percentuálne zastúpenie}%, ...".
        /// 
        /// Výstup je potrebný mať zoradený od najväčších percent po najmenšie,
        /// pričom percentá riaditeľ vyžaduje zaokrúhľovať na jedno desatinné miesto.
        /// Pre zlúčenie musíte podľa jeho slov použiť metódu `Aggregate`.
        /// </summary>
        /// <returns>The query result</returns>
        public string StatisticsAboutGovernmentsQuery()
        {
            var countriesCount = DataContext.Countries.Count;
            
            var governmentType = DataContext.Countries
                .GroupBy(c => c.GovernmentForm)
                .Select(group => new
                {
                    GovernmentForm = group.Key,
                    Count = (double) group.Count() 
                })
                .ToList();

            var result = governmentType
                .OrderByDescending(g => g.Count)
                .Select(g => $"{g.GovernmentForm}: {Math.Round(g.Count / countriesCount * 100, 1):F1}%")
                .Aggregate((acc, curr) => $"{acc}, {curr}");


            Console.WriteLine(result);

            return result;
        }

        /// <summary>
        /// Úloha č. 13
        ///
        /// Firma zistila, že výrobky s tigrovaným vzorom sú veľmi populárne. Chce to preto aplikovať
        /// na svoju doménu.
        ///
        /// Nájdite informácie o ľudoch, ktorí boli obeťami útoku žraloka s menom "Tiger shark"
        /// a útok sa odohral v roku 2001.
        /// Výpis majte vo formáte:
        /// "{meno obete} was tiggered in {krajina, kde sa útok odohral}".
        /// V prípade, že chýba informácia o krajine útoku, uveďte namiesto názvu krajiny reťazec "Unknown country".
        /// V prípade, že informácie o obete vôbec neexistuje, útok ignorujte.
        ///
        /// Ale pozor! Váš nový nadriadený má panický strach z operácie `Join` alebo `GroupJoin`.
        /// Informácie musíte zistiť bez spojenia hocijakých dvoch tabuliek. Skúste sa napríklad zamyslieť,
        /// či by vám pomohla metóda `Zip`.
        /// </summary>
        /// <returns>The query result</returns>
        public List<string> TigerSharkAttackZipQuery()
        {
            var sharkAttack = DataContext.SharkAttacks
                .Where(a => a.DateTime.Value.Year == 2001);

            var tigerSharkAttack = sharkAttack
                .Where(a => DataContext.SharkSpecies.Any(ss => a.SharkSpeciesId == ss.Id && ss.Name == "Tiger shark"))
                .ToList();

            // foreach (var array in tigerSharkAttack) 
            //     Console.WriteLine(string.Join(" ", array));
            
            var attackedPerson = DataContext.AttackedPeople
                .Where(p => tigerSharkAttack.Any(tsa => tsa.AttackedPersonId == p.Id))
                .Select(p => new
                {
                    Name = p.Name,
                    PersonId = p.Id
                });

            // foreach (var array in attackedPerson) 
            //     Console.WriteLine(string.Join(" ", array));


            var countries = tigerSharkAttack
                .Select(a => new
                {
                    Country = DataContext.Countries.FirstOrDefault(c => c.Id == a.CountryId)?.Name ?? "Unknown country"
                });
            
            // foreach (var array in countries.ToList()) 
            //     Console.WriteLine(string.Join(" ", array));
            
            var result = attackedPerson.Zip(countries, (p, c) => new
                {
                    AttackedPerson = p,
                    CountryName = c.Country
                })
                .Select(pc => $"{pc.AttackedPerson.Name} was tiggered in {pc.CountryName}")
                .ToList();
            
            // foreach (var array in result) 
            //     Console.WriteLine(string.Join(" ", array));


            return result;
        }

        /// <summary>
        /// Úloha č. 14
        ///
        /// Vedúci oddelenia prišiel s ďalšou zaujímavou hypotézou. Myslí si, že veľkosť žraloka nejako 
        /// súvisí s jeho apetítom na ľudí.
        ///
        /// Zistite pre neho údaj, koľko percent útokov má na svedomí najväčší a koľko najmenší žralok.
        /// Veľkosť v tomto prípade uvažujeme podľa dĺžky.
        /// 
        /// Výstup vytvorte vo formáte: "{percentuálne zastúpenie najväčšieho}% vs {percentuálne zastúpenie najmenšieho}%"
        /// Percentuálne zastúpenie zaokrúhlite na jedno desatinné miesto.
        /// </summary>
        /// <returns>The query result</returns>
        public string LongestVsShortestSharkQuery()
        {
            var totalAttacks = DataContext.SharkAttacks.Count;

            var orderedSharks = DataContext.SharkSpecies.OrderByDescending(s => s.Length);
            var largestShark = orderedSharks.First();
            var smallestShark = orderedSharks.Last();
            
            var largestSharkAttacks = DataContext.SharkAttacks.Count(sa => sa.SharkSpeciesId == largestShark.Id);
            var smallestSharkAttacks = DataContext.SharkAttacks.Count(sa => sa.SharkSpeciesId == smallestShark.Id);

            var largestSharkPercentage = Math.Round((double)largestSharkAttacks / totalAttacks * 100, 1);
            var smallestSharkPercentage = Math.Round((double)smallestSharkAttacks / totalAttacks * 100, 1);

            var result = $"{largestSharkPercentage:F1}% vs {smallestSharkPercentage:F1}%";

            Console.WriteLine(result);

            return result;
        }

        /// <summary>
        /// Úloha č. 15
        ///
        /// Na koniec vašej kariéry Vám chceme všetci poďakovať a pripomenúť Vám vašu mlčanlivosť.
        /// 
        /// Ako výstup požadujeme počet krajín, v ktorých žralok nespôsobil smrť (útok nebol fatálny).
        /// Berte do úvahy aj tie krajiny, kde žralok vôbec neútočil.
        /// </summary>
        /// <returns>The query result</returns>
        public int SafeCountriesQuery()
        {
            var countryCount = DataContext.Countries.Count;
            Console.WriteLine($"Country {countryCount}");
            
            var fatalAttacks = DataContext.SharkAttacks
                .Where(a => a.AttackSeverenity == AttackSeverenity.Fatal)
                .Join(DataContext.Countries,
                    attack => attack.CountryId,
                    country => country.Id,
                    (attack, country) => new { attack, country })
                .Select(sc => sc.country.Name)
                .Distinct()
                .Count();


            var result = countryCount - fatalAttacks;
            Console.WriteLine(result);
           
            return result;
        }
    }
}
