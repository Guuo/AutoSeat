# Istumajärjestysoptimoija
Blazor WebAssembly -sovellus, joka optimoi istumajärjestyksen sitseille tai muille "pitkän pöydän" juhlille hyödyntäen algoritminään simuloitua jäähdytystä (simulated annealing).

## 💻 Käyttöönotto

Mene osoitteeseen https://guuo.github.io/AutoSeat/ ja seuraa ohjeita!

## 📋 Ominaisuudet

- Optimoi istumajärjestyksen usealle pöydälle, tarvittavien pöytien määrä lasketaan automaattisesti pöytien koon ja osallistujamäärän mukaan
- Istumajärjestys luodaan osallistujien vierustoveritoiveiden mukaan
- Tukee CSV-tiedostojen tuontia (esim. Google Forms -vastaukset)
- Säädettävä pöytäleveys
- Visuaalinen istumajärjestyksen esitys
- Vieruspaikan painotus toiveita täytettäessä: vierekkäin > vastapäätä > viistosti vastapäätä

## 💡 Miten se toimii?

1. Lataa osallistujien toiveet CSV-tiedostona
2. Säädä pöytien leveys
3. Anna algoritmin optimoida istumajärjestys
4. Tarkastele ja tallenna tulos

## 📝 CSV-tiedoston vaatimukset

CSV-tiedostossa tulee olla vähintään kaksi saraketta:
- Osallistujan nimi
- Pilkuilla erotettu lista toivotuista istumanaapureista

## 🛠️ Tekninen toteutus

- Blazor WebAssembly
- Simuloitu jäähdytys optimointialgoritmina
- Selainpohjainen käyttöliittymä
