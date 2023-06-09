﻿using PV178.Homeworks.HW03.DataLoading.DataContext;
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
            var bahamasCountry = DataContext.Countries
                .Where(c => c.Name == "Bahamas");

            var sharkSpeciesLatinNameIorN = DataContext.SharkSpecies
                .Where(s => s.LatinName != null && (s.LatinName.StartsWith("I") || s.LatinName.StartsWith("N")));

            var result = DataContext.SharkAttacks
                .Join(bahamasCountry,
                    attack => attack.CountryId,
                    country => country.Id,
                    (attack, _) => attack)
                .Join(sharkSpeciesLatinNameIorN,
                    attack => attack.SharkSpeciesId,
                    species => species.Id,
                    (attack, species) => new { attack, species })
                .Join(DataContext.AttackedPeople,
                    sa => sa.attack.AttackedPersonId,
                    person => person.Id,
                    (sa, person) => new { sa.species, person })
                .Select(np => $"{np.person.Name} was attacked in Bahamas by {np.species.LatinName}")
                .ToList();

            return result;
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
            var nonDemocracyCountries = DataContext.Countries
                .Where(c => c.GovernmentForm == GovernmentForm.Monarchy ||
                            c.GovernmentForm == GovernmentForm.Territory);

            var nonFatalAttacks = DataContext.SharkAttacks
                .Where(a => a.AttackSeverenity != AttackSeverenity.Fatal);

            var result = nonDemocracyCountries
                .Join(nonFatalAttacks,
                    country => country.Id,
                    attack => attack.CountryId,
                    (country, attack) => new { country, attack })
                .Count();

            return result;
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
            var southAmericaCountries = DataContext.Countries
                .Where(c => !string.IsNullOrEmpty(c.Name) && c.Continent == "South America");

            var sharkSpecies = DataContext.SharkSpecies
                .Where(ss => !string.IsNullOrEmpty(ss.AlsoKnownAs));

            var result = southAmericaCountries
                .Join(DataContext.SharkAttacks,
                    country => country.Id,
                    attack => attack.CountryId,
                    (country, attack) => new { country, attack } )
                .Join(sharkSpecies,
                    ca => ca.attack.SharkSpeciesId,
                    species => species.Id,
                    (ca, species) => new { Country = ca.country, Shark = species})
                .GroupBy(attack => attack.Country.Name)
                .Select(group => new
                {
                    Country = group.Key,
                    AlsoKnownAs = group
                        .GroupBy(cs => cs.Shark.AlsoKnownAs)
                        .OrderByDescending(groupSpecies => groupSpecies.Count())
                        .Select(groupSpecies => groupSpecies.Key)
                        .FirstOrDefault()
                })
                .ToDictionary(
                    cs => cs.Country!, 
                    cs => cs.AlsoKnownAs!
                    );

            return result;
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
            var menAttacks = DataContext.AttackedPeople
                .Where(p => p.Sex == Sex.Male)
                .Join(DataContext.SharkAttacks,
                    person => person.Id,
                    attack => attack.AttackedPersonId,
                    (person, attack) => new { person, attack });

            var result = DataContext.SharkSpecies
                .GroupJoin(menAttacks,
                    species => species.Id,
                    pa => pa.attack.SharkSpeciesId,
                    (species, pa) => new
                    {
                        Species = species,
                        AttackCount = pa.Count()
                    })
                .OrderByDescending(sa => sa.AttackCount)
                .Take(3)
                .Select(sa => sa.Species.Id)
                .ToList();
            
            return result;
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
            var sharkAttacks = DataContext.SharkAttacks
                .Where(a => a.Activity != null && a.Activity.ToLower().Contains("swimming"));

            var result = DataContext.SharkSpecies
                .Where(s => s.TopSpeed.HasValue)
                .Join(sharkAttacks,
                    species => species.Id,
                    attack => attack.SharkSpeciesId,
                    (species, attack) => new { species, attack })
                .Join(DataContext.Countries.Where(c => !string.IsNullOrEmpty(c.Continent)),
                    sa => sa.attack.CountryId,
                    country => country.Id,
                    (sa, country) => new { sa.species, country })
                .GroupBy(sc => sc.country.Continent)
                .ToDictionary(
                    group => group.Key!,
                    group => Math.Round(group.Average(sc => sc.species.TopSpeed!.Value), 2)
                    );

            return result;
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
            var zambesiShark = DataContext.SharkSpecies
                .Where(ss => ss.AlsoKnownAs == "Zambesi shark");

            var attacks = DataContext.SharkAttacks
                .Where(a => a.AttackSeverenity == AttackSeverenity.NonFatal 
                            && a.Type == AttackType.Boating 
                            && a.DateTime >= new DateTime(1960, 3, 3));

            var people = DataContext.AttackedPeople
                .Where(p => p.Name != null
                            && p.Name[0] >= 'D' 
                            && p.Name[0] <= 'K');

            var result = zambesiShark
                .Join(attacks,
                    species => species.Id,
                    attack => attack.SharkSpeciesId,
                    (species, attack) => new { species, attack })
                .Join(people,
                    sa => sa.attack.AttackedPersonId,
                    person => person.Id,
                    (_, person) => person)
                .Select(p => p.Name!)
                .ToList();

            return result;
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
            var lightestSharksAttacks = DataContext.SharkSpecies
                .OrderBy(s => s.Weight)
                .Take(10)
                .Join(DataContext.SharkAttacks,
                    species => species.Id,
                    attack => attack.SharkSpeciesId,
                    (species, attack) => new { species, attack });

            var southAmericanCountries = DataContext.Countries
                .Where(c => !string.IsNullOrEmpty(c.Name) && c.Continent == "South America");
            
            var result = southAmericanCountries
                .Select(country => new Tuple<string, List<SharkSpecies>>(
                    country.Name!,
                    lightestSharksAttacks
                        .Where(sa => sa.attack.CountryId == country.Id)
                        .Select(sa => sa.species)
                        .Distinct()
                        .ToList()
                ))
                .ToList();

            return result;
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
            bool result = DataContext.SharkSpecies
                .Where(species => species.TopSpeed >= 56)
                .All(species => DataContext.SharkAttacks
                    .Where(attack => attack.SharkSpeciesId == species.Id)
                    .Join(DataContext.AttackedPeople,
                        attack => attack.AttackedPersonId,
                        person => person.Id,
                        (_, person) => person)
                    .Any(p => p.Age > 56));
            
            return result;
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
                    (attack, species) => new { attack, species });

            var countries = DataContext.Countries
                .Where(c => !string.IsNullOrEmpty(c.Name) && 
                            (c.Name.StartsWith('B') || c.Name.StartsWith('R')));

            var attackedPeople = DataContext.AttackedPeople
                .Where(p => !string.IsNullOrEmpty(p.Name) && 
                            !char.IsDigit(p.Name[0]) && 
                            char.IsUpper(p.Name[0]));

            var result = fatalAttacks
                .Join(countries,
                    sa => sa.attack.CountryId,
                    country => country.Id,
                    (sa, country) => new { sa, country })
                .Join(attackedPeople,
                    sac => sac.sa.attack.AttackedPersonId,
                    person => person.Id,
                    (sac, person) => new { Shark = sac.sa.species, Country = sac.country, Person = person })
                .Select(scp => $"{scp.Person.Name} was attacked in {scp.Country.Name} by {scp.Shark.LatinName}")
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
                .Where(a => a.AttackSeverenity != AttackSeverenity.Unknown)
                .Join(europeanCountries,
                    attack => attack.CountryId,
                    country => country.Id,
                    (attack, country) => new { attack, country });

            var result = europeanAttacks
                .GroupBy(a => a.country)
                .Select(group => new
                {
                    CountryName = group.Key.Name,
                    Fine = group.Sum(a => a.attack.AttackSeverenity == AttackSeverenity.NonFatal ? 250 : 300),
                    group.Key.CurrencyCode
                })
                .OrderByDescending(cf => cf.Fine)
                .Select(cfc => $"{cfc.CountryName}: {cfc.Fine} {cfc.CurrencyCode}")
                .ToList();

            return result;
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
            var fatalAttacks = DataContext.SharkAttacks
                .Where(a => a.AttackSeverenity == AttackSeverenity.Fatal);

            var result = fatalAttacks
                .Join(DataContext.SharkSpecies.Where(s => !string.IsNullOrEmpty(s.Name)),
                    attack => attack.SharkSpeciesId,
                    species => species.Id,
                    (_, species) => species )
                .GroupBy(ss => ss)
                .Select(group => new
                {
                    SpeciesName = group.Key.Name!,
                    fatalAttacksCount = group.Count()
                })
                .OrderByDescending(sa => sa.fatalAttacksCount)
                .Take(5)
                .ToDictionary(
                    sa => sa.SpeciesName, 
                    sa => sa.fatalAttacksCount
                    );
            
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

            var governmentForm = DataContext.Countries
                .GroupBy(c => c.GovernmentForm)
                .Select(group => new
                {
                    GovernmentForm = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(g => g.Count);
            
            var result = governmentForm
                .Select(g => $"{g.GovernmentForm}: {Math.Round((double) g.Count / countriesCount * 100, 1):F1}%")
                .Aggregate((accumulator, currentForm) => $"{accumulator}, {currentForm}");

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
            var tigerSharkAttack = DataContext.SharkAttacks
                .Where(a => a.DateTime != null 
                            && a.DateTime.Value.Year == 2001 
                            && DataContext.SharkSpecies.Any(ss => a.SharkSpeciesId == ss.Id && ss.Name == "Tiger shark"));

            var attackedPerson = DataContext.AttackedPeople
                .Where(p => tigerSharkAttack.Any(tsa => tsa.AttackedPersonId == p.Id));

            var countries = tigerSharkAttack
                .Select(a => new
                {
                    Country = DataContext.Countries.FirstOrDefault(c => c.Id == a.CountryId)?.Name ?? "Unknown country"
                });
            
            var result = attackedPerson.Zip(countries, (p, c) => new
                {
                    AttackedPerson = p,
                    CountryName = c.Country
                })
                .Select(pc => $"{pc.AttackedPerson.Name} was tiggered in {pc.CountryName}")
                .ToList();

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
            
            var sharkStats = DataContext.SharkSpecies
                .OrderByDescending(s => s.Length)
                .Select(s => new
                {
                    Percentage = Math.Round((double)DataContext.SharkAttacks
                        .Count(sa => sa.SharkSpeciesId == s.Id) / totalAttacks * 100, 1)
                })
                .ToList();

            return $"{sharkStats.First().Percentage:F1}% vs {sharkStats.Last().Percentage:F1}%";
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
            var fatalSharkAttacks = DataContext.SharkAttacks
                .Where(a => a.AttackSeverenity == AttackSeverenity.Fatal);

            var result = DataContext.Countries
                .Where(c => !string.IsNullOrEmpty(c.Name))
                .GroupJoin(fatalSharkAttacks,
                    country => country.Id,
                    attack => attack.CountryId,
                    (country, fatalAttack) => new { country, fatalAttack })
                .Where(ca =>  !ca.fatalAttack.Any())
                .Select(ca => ca.country.Name!)
                .Count();

            return result;
        }
    }
}
