using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
/// <summary>
/// Labyrinttipeli, jossa pelaaja liikkuu läpi kentän. 
/// </summary>
public class LabyrinttiPeli : PhysicsGame
{
    /// <summary>
    /// Pelaaja/Hahmo jolla pelataan
    /// </summary>
    PhysicsObject pelaaja;

    /// <summary>
    /// Pistelaskuri
    /// </summary>
    DoubleMeter pisteLaskuri;
    
    /// <summary>
    /// Aikalaskuri
    /// </summary>
    Timer aikaLaskuri;

    /// <summary>
    /// Maali
    /// </summary>
    PhysicsObject maali;

    /// <summary>
    /// Pistenäytön ja aikanäytön paikka
    /// </summary>
    static readonly Vector[] NayttoPaikat = new Vector[] { new Vector(-447, 360), new Vector(-447, 335) };

    /// <summary>
    /// Ohjenäyttö
    /// </summary>
    static readonly Widget ruutu = new Widget(800.0, 500.0);

    /// <summary>
    /// Valikon tekstien paikat
    /// </summary>
    static readonly Vector[] ValikonPaikat = new Vector[] { new Vector(0, 40), new Vector(0, 0), new Vector(0, -40), new Vector(0, -80), new Vector(0, -120), new Vector(0, -160), new Vector(0, -300) };

    /// <summary>
    /// Pelaajan liikuttamisen vektorit
    /// </summary>
    static readonly Vector[] Pelaaja_Liikkuu = new Vector[] { new Vector(0, 2000), new Vector(2000, 0), new Vector(-2000, 0), new Vector(0, -2000) };

    /// <summary>
    /// Este
    /// </summary>
    PhysicsObject este;

    /// <summary>
    /// Ruutujen koko pelissä
    /// </summary>
    const double RuutuKoko = 20;

    /// <summary>
    /// Kokonaispisteet
    /// </summary>
    const int kokonaisPisteet = 10000;

    /// <summary>
    /// Tahtipisteet
    /// </summary>
    const int tahtiPisteet = 1700;

    /// <summary>
    /// Lista -valikko
    /// </summary>
    List<Label> valikonKohdat;

    /// <summary>
    /// HighScore -listat kentille 1-5
    /// </summary>
    static ScoreList[] topListat = new ScoreList[] { new ScoreList(5, false, 0), new ScoreList(5, false, 0), new ScoreList(5, false, 0), new ScoreList(5, false, 0), new ScoreList(5, false, 0) };

    /// <summary>
    /// Kenttien numerointi
    /// </summary>
    int kenttaNro;

    /// <summary>
    /// Taulukko pisteiden laskemiseksi seinien tuhoamisesta
    /// </summary>
    readonly double[] seinaPisteet = new double[] {1000, 1000, 1000, 1000, 1000 };


    /// <summary>
    /// Ohjelma, joka aloittaa pelin lataamalla kenttien HighScoret ja avaamalla "Valikon".
    /// </summary>
    public override void Begin()
    {
        topListat[0] = DataStorage.TryLoad<ScoreList>(topListat[0], "pisteet1.xml");
        topListat[1] = DataStorage.TryLoad<ScoreList>(topListat[1], "pisteet2.xml");
        topListat[2] = DataStorage.TryLoad<ScoreList>(topListat[2], "pisteet3.xml");
        topListat[3] = DataStorage.TryLoad<ScoreList>(topListat[3], "pisteet4.xml");
        topListat[4] = DataStorage.TryLoad<ScoreList>(topListat[4], "pisteet5.xml");
        Valikko();
    }



    /// <summary>
    /// Tässä osiossa käynnistetään/ajetaan kentät 1-5
    /// </summary>
    //#################################################################################
    //#                     !! KENTÄT !!                                              #
    //#################################################################################


    /// <summary>
    /// Aliohjelma, joka käynnistää kentät 1-5
    /// </summary>
    public void Kentta(int numero)
    {

        kenttaNro = numero;
        ClearAll();
        LuoKentta(kenttaNro = numero);
        AsetaOhjaimet();
        LuoPistelaskuri();
        LuoAikaLaskuri();

        Camera.ZoomToLevel();
    }

 

    /// <summary>
    /// Tässä osiossa olevissa aliohjelmissa luodaan kentät 1-5
    /// </summary>
    //#################################################################################
    //#                     !! KENTTIEN LUOMINEN !!                                   #
    //#################################################################################


    /// <summary>
    /// Aliohjelma, joka luo kentän
    /// </summary>
    public void LuoKentta(int kenttaNro)
    {
        ColorTileMap ruudut = ColorTileMap.FromLevelAsset("kentta" + kenttaNro);
        ruudut.SetTileMethod(Color.Black, LuoTaso);
        ruudut.Execute(RuutuKoko, RuutuKoko);

        ColorTileMap seinat = ColorTileMap.FromLevelAsset("kentta" + kenttaNro);
        seinat.SetTileMethod(Color.Green, LuoSeina);
        seinat.Execute(RuutuKoko, RuutuKoko);

        ColorTileMap ukko = ColorTileMap.FromLevelAsset("kentta" + kenttaNro);
        ukko.SetTileMethod(Color.Blue, LuoPelaaja);
        ukko.Execute(RuutuKoko, RuutuKoko);

        ColorTileMap tahdet = ColorTileMap.FromLevelAsset("kentta" + kenttaNro);
        tahdet.SetTileMethod(Color.Yellow, LuoTahti);
        tahdet.Execute(RuutuKoko, RuutuKoko);

        ColorTileMap maali = ColorTileMap.FromLevelAsset("kentta" + kenttaNro);
        maali.SetTileMethod(Color.Red, LuoMaali);
        maali.Execute(RuutuKoko, RuutuKoko);

        ColorTileMap painike = ColorTileMap.FromLevelAsset("kentta" + kenttaNro);
        painike.SetTileMethod(Color.Aqua, LuoPainike);
        painike.Execute(RuutuKoko, RuutuKoko);

        ColorTileMap este = ColorTileMap.FromLevelAsset("kentta" + kenttaNro);
        este.SetTileMethod(new Color(165, 42, 42), LuoEste);
        este.Execute(RuutuKoko, RuutuKoko);

        ColorTileMap vihollinen = ColorTileMap.FromLevelAsset("kentta" + kenttaNro);
        vihollinen.SetTileMethod(new Color(255, 0, 220), LuoVihollinen);
        vihollinen.Execute(RuutuKoko, RuutuKoko);
    }



    /// <summary>
    /// Tässä osiossa olevat aliohjelmat luovat erilaiset esineet, hahmot sekä tasot
    /// </summary>
    //#################################################################################
    //#                     !! LUO ESINEET, HAHMOT, TASOT !!                          #
    //#################################################################################


    /// <summary>
    /// Aliohjelma, joka luo painikkeen/vivun
    /// </summary>
    public void LuoPainike(Vector paikka, double korkeus, double leveys)
    {
        PhysicsObject painike = PhysicsObject.CreateStaticObject(RuutuKoko * 4, RuutuKoko * 4);
        painike.Position = paikka;
        painike.Image = LoadImage("painike");
        painike.Tag = "painike";
        Add(painike);
    }


    /// <summary>
    /// Aliohjelma, joka luo esteet
    /// </summary>
    public void LuoEste(Vector paikka, double korkeus, double leveys)
    {
        este = PhysicsObject.CreateStaticObject(RuutuKoko * 5, RuutuKoko * 5);
        este.Position = paikka;
        este.Image = LoadImage("este");
        //este.CollisionIgnoreGroup = 1;
        este.Tag = "este";
        Add(este);
    }


    /// <summary>
    /// Aliohjelma, joka luo staattiset tasot
    /// </summary>
    public void LuoTaso(Vector paikka, double korkeus, double leveys)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Shape = Shape.Rectangle;
        taso.Color = Color.Black;
        taso.CollisionIgnoreGroup = 1;
        Add(taso);
    }


    /// <summary>
    /// Aliohjelma, joka luo seinät joita voi tuhota
    /// </summary>
    public void LuoSeina(Vector paikka, double korkeus, double leveys)
    {
        PhysicsObject seina = PhysicsObject.CreateStaticObject(korkeus, leveys);
        seina.Position = paikka;
        seina.Shape = Shape.Rectangle;
        seina.Color = Color.BrownGreen;
        seina.CollisionIgnoreGroup = 1;
        seina.Tag = "seina";
        Add(seina);
    }


    /// <summary>
    /// Aliohjelma, joka luo pelaajan/hahmon jolla pelataan
    /// </summary>
    public void LuoPelaaja(Vector paikka, double korkeus, double leveys)
    {
        pelaaja = new PhysicsObject(RuutuKoko * 4, RuutuKoko * 4);
        pelaaja.Position = paikka;
        pelaaja.Image = LoadImage("hahmo");
        pelaaja.CanRotate = false;
        pelaaja.LinearDamping = 0.3;
        Add(pelaaja);

        AddCollisionHandler(pelaaja, "tahti", PelaajaTormaaTahteen);
        AddCollisionHandler(pelaaja, "seina", PelaajaTormasiSeinaan);
        AddCollisionHandler(pelaaja, "maali", PelaajaPaaseeMaaliin);
        AddCollisionHandler(pelaaja, "painike", PelaajaTormaaPainikkeeseen);
        AddCollisionHandler(pelaaja, "vihollinen", PelaajaTormaaViholliseen);
    }


    /// <summary>
    /// Aliohjelma, joka luo 3 tähteä jotka voi kerätä
    /// </summary>
    public void LuoTahti(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tahti = PhysicsObject.CreateStaticObject(RuutuKoko * 4, RuutuKoko * 4);
        tahti.Position = paikka;
        tahti.Image = LoadImage("tahti");
        tahti.Tag = "tahti";
        Add(tahti);
    }


    /// <summary>
    /// Aliohjelma, joka luo vihollisen
    /// </summary>
    public void LuoVihollinen(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject vihollinen = new PhysicsObject(RuutuKoko * 4, RuutuKoko * 4);
        vihollinen.Position = paikka;
        vihollinen.Image = LoadImage("vihollinen");
        vihollinen.Tag = "vihollinen";
        Add(vihollinen);

        RandomMoverBrain satunnaisAivot = new RandomMoverBrain(300);
        satunnaisAivot.ChangeMovementSeconds = 1.5;
        satunnaisAivot.WanderRadius = 200;
        vihollinen.Brain = satunnaisAivot;
    }



    /// <summary>
    /// Tässä osiossa olevat aliohjelmat luovat laskurit(piste- ja aika-) sekä maalin
    /// </summary>
    //#################################################################################
    //#                     !! LUO LASKURIT JA MAALI !!                               #
    //#################################################################################


    /// <summary>
    /// Aliohjelma, joka luo pistelaskurin
    /// </summary>
    public void LuoPistelaskuri()
    {
        pisteLaskuri = new DoubleMeter(0);

        Label pisteNaytto = new Label();
        pisteNaytto.Position = NayttoPaikat[0];
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.Title = "Pisteet";

        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
    }


    /// <summary>
    /// Aliohjelma, joka luo aikalaskurin
    /// </summary>
    public void LuoAikaLaskuri()
    {
        aikaLaskuri = new Timer();
        aikaLaskuri.Start();

        Label aikaNaytto = new Label();
        aikaNaytto.Position = NayttoPaikat[1];
        aikaNaytto.TextColor = Color.Black;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.BindTo(aikaLaskuri.SecondCounter);
        //aikaNaytto.Title = "aika";
        Add(aikaNaytto);

    }

    /// <summary>
    /// Aliohjelma, joka luo maalin
    /// </summary>
    public void LuoMaali(Vector paikka, double leveys, double korkeus)
    {
        maali = PhysicsObject.CreateStaticObject(RuutuKoko * 7, RuutuKoko * 7);
        maali.Position = paikka;
        maali.Image = LoadImage("maali");
        maali.Tag = "maali";
        Add(maali);
    }



    /// <summary>
    /// Tässä osiossa olevat aliohjelmat luovat valikot, taulukot ja ohje -sivun
    /// </summary>
    //#################################################################################
    //#                     !! LUO VALIKOT, TAULUKOT JA OHJEET !!                     #
    //#################################################################################


    /// <summary>
    /// Aliohjelma, joka luo lopputaulukon
    /// </summary>
    public void Lopputaulukko()
    {
        MultiSelectWindow valikko = new MultiSelectWindow("Pääsit maaliin", "Seuraava kenttä", "Pelaa uudestaan", "Poistu valikkoon");
        valikko.ItemSelected += PainettiinValikonNapppia;
        Add(valikko);
    }


    /// <summary>
    /// Aliohjelma, joka luo valikon
    /// </summary>
    public void Valikko()
    {
        ClearAll();

        valikonKohdat = new List<Label>();

        Label kohta1 = new Label("Kentät");
        kohta1.Position = ValikonPaikat[0];
        valikonKohdat.Add(kohta1);

        Label kohta2 = new Label("Parhaat pisteet");
        kohta2.Position = ValikonPaikat[1];
        valikonKohdat.Add(kohta2);

        Label kohta3 = new Label("Ohjeet");
        kohta3.Position = ValikonPaikat[2];
        valikonKohdat.Add(kohta3);

        Label kohta4 = new Label("Lopeta peli");
        kohta4.Position = ValikonPaikat[3];
        valikonKohdat.Add(kohta4);

        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, Kentat, "Avaa kenttä valikon");
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, ParhaatPisteet, "Avaa parhaiden pisteiden valikon");
        Mouse.ListenOn(kohta3, MouseButton.Left, ButtonState.Pressed, Ohjeet, "Avaa ohjeet");
        Mouse.ListenOn(kohta4, MouseButton.Left, ButtonState.Pressed, Exit, "Lopettaa pelin");


        Mouse.ListenMovement(1.0, ValikossaLiikkuminen, "Muuttaa tekstin väriä kun hiiri menee valikossa sen päälle");
    }


    /// <summary>
    /// Aliohjelma, joka luo valikon josta valitaan kentät
    /// </summary>
    public void Kentat()
    {
        ClearAll();

        valikonKohdat = new List<Label>();

        Label kohta1 = new Label("Kenttä 1");
        kohta1.Position = ValikonPaikat[0];
        valikonKohdat.Add(kohta1);

        Label kohta2 = new Label("Kenttä 2");
        kohta2.Position = ValikonPaikat[1];
        valikonKohdat.Add(kohta2);

        Label kohta3 = new Label("Kenttä 3");
        kohta3.Position = ValikonPaikat[2];
        valikonKohdat.Add(kohta3);

        Label kohta4 = new Label("Kenttä 4");
        kohta4.Position = ValikonPaikat[3];
        valikonKohdat.Add(kohta4);

        Label kohta5 = new Label("Kenttä 5");
        kohta5.Position = ValikonPaikat[4];
        valikonKohdat.Add(kohta5);

        Label kohta6 = new Label("Palaa valikkoon");
        kohta6.Position = ValikonPaikat[5];
        valikonKohdat.Add(kohta6);

        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, delegate { Kentta(1); }, "Käynnistää kentän 1");
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, delegate { Kentta(2); }, "Käynnistää kentän 2");
        Mouse.ListenOn(kohta3, MouseButton.Left, ButtonState.Pressed, delegate { Kentta(3); }, "Käynnistää kentän 3");
        Mouse.ListenOn(kohta4, MouseButton.Left, ButtonState.Pressed, delegate { Kentta(4); }, "Käynnistää kentän 4");
        Mouse.ListenOn(kohta5, MouseButton.Left, ButtonState.Pressed, delegate { Kentta(5); }, "Käynnistää kentän 5");
        Mouse.ListenOn(kohta6, MouseButton.Left, ButtonState.Pressed, Valikko, "Palaa valikkoon");

        Mouse.ListenMovement(1.0, ValikossaLiikkuminen, "Muuttaa tekstin väriä kun hiiri menee valikossa sen päälle");
    }


    /// <summary>
    /// Aliohjelma, joka näyttää ohjeet
    /// </summary>
    public void Ohjeet()
    {
        ClearAll();

        ruutu.BorderColor = Color.Black;
        ruutu.Image = LoadImage("ohjeet");
        Add(ruutu);

        valikonKohdat = new List<Label>();

        Label kohta1 = new Label("Palaa valikkoon");
        kohta1.Position = ValikonPaikat[6];
        valikonKohdat.Add(kohta1);

        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, Valikko, "Palaa valikkoon");
        Mouse.ListenMovement(1.0, ValikossaLiikkuminen, "Muuttaa tekstin väriä kun hiiri menee valikossa sen päälle");
    }


    /// <summary>
    /// Aliohjelma, joka luo pausetaulukon joka näkyy pause nappia painamalla
    /// </summary>
    public void Pausetaulukko()
    {
        double aikaaKulunut = aikaLaskuri.SecondCounter.Value;
        aikaLaskuri.Pause();

        MultiSelectWindow valikko = new MultiSelectWindow("Peli on seis", "Jatka peliä", "Pelaa uudestaan", "Poistu valikkoon");
        valikko.ItemSelected += PainettiinPauseValikonNapppia;
        Add(valikko);
    }



    /// <summary>
    /// Tähän osioon on laitettu kaikki aliohjelmat, joihin liittyy pisteet (niiden laskeminen ja tallentaminen) sekä pistetaulukot esim. HighScore -listat
    /// </summary>
    //#################################################################################
    //#                     !! PISTEET JA PISTETAULUKOT !!                            #
    //#################################################################################


    /// <summary>
    /// Aliohjelma, joka luo valikon josta pääsee näkemään HighScoret
    /// </summary>
    public void ParhaatPisteet()
    {
        ClearAll();

        valikonKohdat = new List<Label>();

        Label kohta1 = new Label("Kenttä 1");
        kohta1.Position = ValikonPaikat[0];
        valikonKohdat.Add(kohta1);

        Label kohta2 = new Label("Kenttä 2");
        kohta2.Position = ValikonPaikat[1];
        valikonKohdat.Add(kohta2);

        Label kohta3 = new Label("Kenttä 3");
        kohta3.Position = ValikonPaikat[2];
        valikonKohdat.Add(kohta3);

        Label kohta4 = new Label("Kenttä 4");
        kohta4.Position = ValikonPaikat[3];
        valikonKohdat.Add(kohta4);

        Label kohta5 = new Label("Kenttä 5");
        kohta5.Position = ValikonPaikat[4];
        valikonKohdat.Add(kohta5);

        Label kohta6 = new Label("Palaa valikkoon");
        kohta6.Position = ValikonPaikat[5];
        valikonKohdat.Add(kohta6);

        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, delegate { Pisteet(1); }, "Avaa ensimmäisen kentän parhaiden pisteiden listan");
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, delegate { Pisteet(2); }, "Avaa toisen kentän parhaiden pisteiden listan");
        Mouse.ListenOn(kohta3, MouseButton.Left, ButtonState.Pressed, delegate { Pisteet(3); }, "Avaa kolmannen kentän parhaiden pisteiden listan");
        Mouse.ListenOn(kohta4, MouseButton.Left, ButtonState.Pressed, delegate { Pisteet(4); }, "Avaa neljännen kentän parhaiden pisteiden listan");
        Mouse.ListenOn(kohta5, MouseButton.Left, ButtonState.Pressed, delegate { Pisteet(5); }, "Avaa viidennen kentän parhaiden pisteiden listan");
        Mouse.ListenOn(kohta6, MouseButton.Left, ButtonState.Pressed, Valikko, "Palaa valikkoon");

        Mouse.ListenMovement(1.0, ValikossaLiikkuminen, "Muuttaa tekstin väriä kun hiiri menee valikossa sen päälle");
    }

    public void Pisteet(int numero)
    {
        HighScoreWindow topIkkuna = new HighScoreWindow("Parhaat pisteet: Kenttä " + numero, topListat[numero - 1]);
        topIkkuna.Closed += TallennaPisteet;
        Add(topIkkuna);
    }

    /// <summary>
    /// Aliohjelma, joka laskee kokonaispisteet ja palauttaa pistetilanteen
    /// </summary>
    public void KokonaisPisteet()
    {
        double aikaaKulunut = aikaLaskuri.SecondCounter.Value;
        double p = kokonaisPisteet;

        for (double i = 0; i <= aikaaKulunut; i += 0.5)
        {
            p -= 10;
        }
        pisteLaskuri.Value = +p;

        pisteLaskuri.Value += seinaPisteet[kenttaNro -1];
    }


    /// <summary>
    /// Aliohjelma, joka tallentaa pisteet HighScore -listoille ja niiden xml -tiedostoihin
    /// </summary>
    public void TallennaPisteet(Window sender)
    {
        //DataStorage.Save<ScoreList>(topListat[kenttaNro - 1], "pisteet" + kenttaNro +".xml"); < toimii vain silloin kun kenttä on avattu ennen pisteiden tarkastelua

        if (kenttaNro == 1)
        {
            DataStorage.Save<ScoreList>(topListat[0], "pisteet1.xml");
        }
        if (kenttaNro == 2)
        {
            DataStorage.Save<ScoreList>(topListat[1], "pisteet2.xml");
        }
        if (kenttaNro == 3)
        {
            DataStorage.Save<ScoreList>(topListat[2], "pisteet3.xml");
        }
        if (kenttaNro == 4)
        {
            DataStorage.Save<ScoreList>(topListat[3], "pisteet4.xml");
        }
        if (kenttaNro == 5)
        {
            DataStorage.Save<ScoreList>(topListat[4], "pisteet5.xml");
        }
    }



    /// <summary>
    /// Tähän osioon on laitettu kaikki aliohjelmat, joihin liittyy pelaajan liikuttaminen, pelinohjaimet ja muut napit esim. pausevalikon -napit
    /// </summary>
    //#################################################################################
    //#                     !! LIIKUTTAMINEN, OHJAIMET JA NAPIT !!                    #
    //#################################################################################


    /// <summary>
    /// Aliohjelma pelaajan liikuttamista varten
    /// </summary>
    private void LiikutaPelaajaa(PhysicsObject pelaaja, Vector suunta) 
    {
        pelaaja.Move(suunta);
    }


    /// <summary>
    /// Aliohjelma, joka asettaa ohjaimet pelaajalle sekä peliin
    /// </summary>
    public void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Pausetaulukko, "Poistu"); // Toiminnot näppäimille. Toiminnot kutsuvat aliohjelmaa "LiikutaPelaajaa"
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä näppäinohjeet");
        Keyboard.Listen(Key.Up, ButtonState.Down, LiikutaPelaajaa, "Liiku ylöspäin", pelaaja, Pelaaja_Liikkuu[0]);
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaPelaajaa, "Liiku oikealle", pelaaja, Pelaaja_Liikkuu[1]);
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaPelaajaa, "Liiku vasemmalle", pelaaja, Pelaaja_Liikkuu[2]);
        Keyboard.Listen(Key.Down, ButtonState.Down, LiikutaPelaajaa, "Liiku alaspäin", pelaaja, Pelaaja_Liikkuu[3]);
    }


    /// <summary>
    /// Aliohjelma pausevalikon nappien painamista varten
    /// </summary>
    public void PainettiinPauseValikonNapppia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                PeliJatkuu();
                break;
            case 1:
                if (kenttaNro == 1)
                {
                    Kentta(1);
                }
                if (kenttaNro == 2)
                {
                    Kentta(2);
                }
                if (kenttaNro == 3)
                {
                    Kentta(3);
                }
                if (kenttaNro == 4)
                {
                    Kentta(4);
                }
                if (kenttaNro == 5)
                {
                    Kentta(5);
                }
                break;
            case 2:
                Valikko();
                break;
        }
    }


    /// <summary>
    /// Aliohjelma jonka avulla peli jatkuu kun se laitetaan pauselle
    /// </summary>
    public void PeliJatkuu()
    {
        double aikaaKulunut = aikaLaskuri.SecondCounter.Value;
        aikaLaskuri.Start();
    }



    /// <summary>
    /// Tähän osioon on laitettu kaikki aliohjelmat, joihin liittyy erilaisia tapahtumia esim. törmäykset
    /// </summary>
    //#################################################################################
    //#                     !! TAPAHTUMAT JA TÖRMÄYKSET !!                            #
    //#################################################################################


    /// <summary>
    /// Aliohjelma, joka suoritetaan kun pelaaja törmää seinään. Aliohjelma tekee räjähdyksen ja tuhoaa seinan johon osutaan
    /// </summary>
    public void PelaajaTormasiSeinaan(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        Explosion rajahdys = new Explosion(20);
        rajahdys.Position = kohde.Position;
        rajahdys.Speed = 500.0;
        rajahdys.Force = 0;
        rajahdys.Sound = null;
        Add(rajahdys);

        seinaPisteet[kenttaNro -1] -= 0.5;
        kohde.Destroy();
    }


    /// <summary>
    /// Aliohjelma, joka suoritetaan kun pelaaja törmää painikkeeseen. Aliohjelma tuhoaa esteen
    /// </summary>
    public void PelaajaTormaaPainikkeeseen(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        este.Destroy();
    }


    /// <summary>
    /// Aliohjelma, joka suoritetaan kun pelaaja törmää tähteen. Aliohjelma tuhoaa tähden ja antaa pelaajalle pisteitä
    /// </summary>
    public void PelaajaTormaaTahteen(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        kohde.Destroy();
        pisteLaskuri.Value += tahtiPisteet;
    }


    /// <summary>
    /// Aliohjelma, joka suoritetaan kun pelaaja törmää viholliseen. Aliohjelma aloittaa kentän alusta
    /// </summary>
    public void PelaajaTormaaViholliseen(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        Kentta(5);
    }


    /// <summary>
    /// Aliohjelma, joka suoritetaan kun pelaaja pääsee maaliin. Aliohjelma lopettaa kentän ja avaa valikon sekä HighScore -listan. 
    /// Jos pelaaja saa tuloksen joka on 5 parhaan tuloksen joukossa pääsee pelaaja kirjoittamaan nimensä HighScore -listalle
    /// </summary>
    public void PelaajaPaaseeMaaliin(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        double aikaaKulunut = aikaLaskuri.SecondCounter.Value;

        Label aika = new Label("Aikasi on " + AikaLaskurinAjanPyoristys(aikaaKulunut) + " sekunttia");
        aika.Y = 300;
        aika.Color = Color.White;
        aika.BorderColor = Color.Black;
        Add(aika);

        Label keratytTahdet = new Label("Keräsit " + MontakoTahtea(pisteLaskuri) + " tähteä");
        keratytTahdet.Y = 270;
        keratytTahdet.Color = Color.White;
        keratytTahdet.BorderColor = Color.Black;
        Add(keratytTahdet);

        aikaLaskuri.Pause();
        KokonaisPisteet();

        HighScoreWindow topIkkuna = new HighScoreWindow("Parhaat pisteet", "Onneksi olkoon, pääsit listalle pisteillä %p! Syötä nimesi:", topListat[kenttaNro - 1], pisteLaskuri);
        topIkkuna.Closed += TallennaPisteet;
        Add(topIkkuna);

        Lopputaulukko();
    }


    /// <summary>
    /// Aliohjelma, valikonnappien painamista varten. Aliohjelma kertoo mitä tapahtuu kun tiettyä nappia painetaan
    /// </summary>
    public void PainettiinValikonNapppia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                Seuraavakentta();
                break;
            case 1:
                if (kenttaNro == 1)
                {
                    Kentta(1);
                }
                if (kenttaNro == 2)
                {
                    Kentta(2);
                }
                if (kenttaNro == 3)
                {
                    Kentta(3);
                }
                if (kenttaNro == 4)
                {
                    Kentta(4);
                }
                if (kenttaNro == 5)
                {
                    Kentta(5);
                }
                break;
            case 2:
                Valikko();
                break;
        }
    }


    /// <summary>
    /// Aliohjelma valikossa liikkumista varten. Aliohjelma muuttaa tekstin mustasta punaiseksi kun hiiri laitetaan kohdan päälle
    /// </summary>
    public void ValikossaLiikkuminen()
    {
        foreach (Label kohta in valikonKohdat)
        {
            if (Mouse.IsCursorOn(kohta))
            {
                kohta.TextColor = Color.Red;
            }
            else
            {
                kohta.TextColor = Color.Black;
            }

        }
    }


    /// <summary>
    /// Aliohjelma, jonka avulla voidaan kentän päätyttyä painaa "seuraava kenttä" -nappia. Täten suoritetaan seuraava kenttä.
    /// Jos seuraavaa kenttää ei ole(kenttä 5 -case), painike avaa aloitus-/päävalikon
    /// </summary>
    public void Seuraavakentta()
    {
        if (kenttaNro == 1)
        {
            Kentta(2);
        }
        else if (kenttaNro == 2)
        {
            Kentta(3);
        }
        else if (kenttaNro == 3)
        {
            Kentta(4);
        }
        else if (kenttaNro == 4)
        {
            Kentta(5);
        }
        else if (kenttaNro == 5)
        {
            Valikko();
        }
    }


    /// <summary>
    /// Aliohjelmat joissa on funktio, joka palauttaa arvon kutsuttaessa
    /// </summary>
    //#################################################################################
    //#                     !! FUNKTIOT !!                                            #
    //#################################################################################


    /// <summary>
    /// Muuttaa aikalaskurin ajan kokonaisluvuksi eli poistaa pilkun ja desimaalit
    /// </summary>
    /// <param name="aikaaKulunut">aikalaskurissa oleva aika maalinpäästessä</param>
    /// <returns>aikasi</returns>
    /// <example>
    /// <pre name="test">
    ///   LabyrinttiPeli.AikaLaskurinAjanPyoristys(14,38547) === 14;
    ///   LabyrinttiPeli.AikaLaskurinAjanPyoristys(27,9348) === 27;
    /// </pre>
    /// </example>
    public static string AikaLaskurinAjanPyoristys(double aikaaKulunut)
    {
        string aikasi = aikaaKulunut.ToString();

        if (aikasi.Contains(","))
        {
            aikasi = aikasi.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        }
        if (aikasi.EndsWith(","))
        {
            aikasi = aikasi.TrimEnd(',');
        }
        return aikasi;
    }


    /// <summary>
    /// Muuttaa tähdistä saadut pisteet tähtien määräksi
    /// </summary>
    /// <param name="pisteet">kuinka paljon pisteitä tähdistä on saatu</param>
    /// <returns>tahtienMaara</returns>
    /// <example>
    /// <pre name="test">
    ///   LabyrinttiPeli.MontakoTahtea(4100) === 3;
    ///   LabyrinttiPeli.MontakoTahtea(17000) === 10;
    /// </pre>
    /// </example>
    public static double MontakoTahtea(double pisteet)
    {
        double tahtienMaara = pisteet / tahtiPisteet;

        return tahtienMaara;
    }
}
