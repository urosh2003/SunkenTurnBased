# Primena softverskih obrazaca u razvoju video igre

## Opis igre

U pitanju je strateška video igra istraživanja ukletog potopljenog broda, zasnovana na potezima.  
Tabla, po kojoj se igrač kreće je heksagonalni grid, i sve se zasniva na njemu.  
Svakog poteza, igrač ima određen broj "akcionih poena" (AP) na raspolaganju, koje može da upotrebi da bi ostvario različite akcije koje su mu dostupne.  
Uvek su mu dostupne akcije kretanja i akcija osnovog napada, a ostale akcije je potrebno da otključa kroz igru.  
Takođe, igrač može da naiđe na posebne vrste blaga, koje mu daju trajne bonuse koji mu daju pasivne bonuse i mogu da menjaju kako neke akcije funkcionišu.  
Kroz istraživanje broda, igrač će povremeno biti napadnut od strane neprijatelja, koji su uglavnom morske životinje, gde će se borba zasnivati potezima u krug.  
Igrač će biti ograničen kiseonikom, i trošiće jednu jedinicu kiseonika po potezu, što će ga navoditi da što efikasnije iskoristi svoje poteze.  
Kada mu ponestane kiseonika, igrač mora da izroni iz broda kako bi obnovio kiseonik, ali kada se vrati nazad u brod, videće da ništa nije kao isto, i da su sve životinje postale monstruoznije verzije sebe.

## Arhitektura

### Character

`Character` je apstraktna klasa koja opisuje koja sve osobine, obeležja i koje metode treba da sadrži karakter.

**Najbitnija obeležja:**
- `currentAP`, `perTurnAP`, `maxAP` - koliko karakter trenutno ima akcionih poena, koliko AP dobija po potezu i koliko može maksimalno da ih ima
- `maxHealth` i `currentHealth` - označavaju trenutni i maksimalni broj životnih poena

Character klasu nasleđuju `PlayerCharacter` i `NonPlayerCharacter` klase, koje detaljnije opisuju atribute igrača i neprijatelja, ali više o tome kasnije.

### Akcije

Svaka akcija koju igrač ili neprijatelji mogu da izvedu, implementirana je svojom klasom.

Svaka akcija nasleđuje baznu apstraktnu klasu `IAction`, koja daje okvir za šta treba da sadrži jedna akcija i sve akcije rađene su po **Command** šablonu.  
Svaka akcija sama definiše koji su uslovi da se izvrši, koja je logika za ažuriranje konteksta, kako se tačno izvršava, i kako treba da se UI izmeni prilikom pripreme i izvođenja akcije.  
Akcije takođe pozivaju hookove za gotovo svaku aktivnost karaktera (kretanje, udaranje, pomeranje neprijatelja itd.), koje posle koriste drugi sistemi, po **Template** šablonu.

```csharp
public abstract class IAction
{
    public int APcost;                  // Koliko akcionih poena (AP) nam je potrebno da izvršimo akciju
    public int baseAPcost;              // Kolika je osnovna cena akcije (bez uračunatih izmena cene koje se mogu desiti tokom igre)

    public Character actor;             // Koji karakter pokušava da izvrši akciju

    public ActionContext context;       // Sadrži informacije o tome ko je akter, ko je meta, na kom polju je cursor kojim ciljamo itd.

    public int range;                   // Domet akcije izražen u dužini puta na heksagonalnoj mreži
    public bool resolving = false;      // Da li se akcija trenutno izvršava ili se još uvek priprema, bitno za usklađivanje UI elemenata i animacija
    public int cooldown;                // Koliko poteza je potrebno da prođe pre nego što karakter može ponovo da iskoristi ovu akciju

    public ActionData actionData;       // ScriptableObject preko kojeg čuvamo vrednosti svake akcije kao što su ime, opis, ikonica za ui itd.

    public abstract Task<bool> Execute(); // Izvrši akciju na osnovu trenutnog konteksta. Vraća bool, koji predstavlja da li se akcija uspešno izvršila. Task zbog sinhronizacije.
    
    public virtual bool UpdateContext(ActionContext newContext) // Bazno ažuriranje kontekst ukoliko je drugačiji od trenutnog i ukoliko akcija nije u režimu izvržavanja
    {                                                           // Vraća boolean koji predstavlja da li je došlo do promene konteksta ili ne
        if (this.context.Equals(newContext) || resolving)      // Virtual jer će mnoge akcije same definisati kada i da li može da se promeni kontekst
        {
            return false;
        }
        else
        {
            this.context = newContext;
            return true;
        }
    }

    public IAction(Character actor)    // Bazni konstruktor koji samo naznačava ko je aktor akcije
    {
        this.actor = actor;
    }

    public abstract void DrawTiles();   /* Funkcije koje ažuriraju UI heksagonalne table prilikom pripreme akcije, ne crtaju ga one već delegiraju posao SelectedTilesManager-u
    public abstract void RedrawTiles();  i samo mu opisno govore šta da nacrta, funkcijama poput DrawCircle, DrawSingle itd. */
}
```
Primer implementacije jedne akcije, akcije obicnog napada:

```csharp
public class BasicAttackAction : IAction
{
    Vector3Int actorPosition;
    public BasicAttackAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = actor.basicAttackRange;
        this.cooldown = 0;
        this.baseAPcost = 2;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range
            && this.actor.currentAP >= this.APcost &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) != actor &&
            !resolving
            )
        {
            resolving = true;
            SelectedTilesManager.instance.LockHighlights();
            Character target = GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile);
            if (target != null)
            {
                if (actor is PlayerCharacter)
                {
                    await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, target.transform);
                }
                int damage = await CalculateDamage();
                target.TakeDamage(damage);
                actor.CharacterDamagedEnemy(target, damage);
                if (target == null || target.currentHealth <= 0)
                {
                    actor.CharacterKilledEnemy();
                }
            }
            this.actor.ChangeAP(-this.APcost);
            this.actor.CharacterAttacked(new List<Character> { target });
            if (actor is PlayerCharacter)
            {
               await CameraActionFocus.instance.MinigameDone();
            }
            resolving = false;
            return true;
        }
        return false;
    }

    private async Task<int> CalculateDamage()
    {
        int damage = actor.basicAttackDamage;
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameOne();
            if (results[0])
            {
                damage += minigameBonusDamage;
            }
        }
        return damage + bonusDamage;
    }

    public override void RedrawTiles()
    { 
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) != actor &&
            !resolving
            )
        {
            SelectedTilesManager.instance.DrawSingle(this.context.targetedTile, new TileStyle(TileColor.YELLOW, TileType.XTILE, TileLayer.TARGETING));
        }
        else if (!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.DrawCircle(actorPosition, this.range, new TileStyle(TileColor.YELLOW, TileType.DEFAULT, TileLayer.RANGE));
    }
}
```

Veliki broj hook-ova poput CharacterAttacked, CharacterDamagedEnemy itd. je rasporedjen kroz svaku akciju.
Te hookove ce posle koristiti drugi sistemi o kojima ce biti vise reci posle. 

### Player

IPlayerState je apstraktna klasa koja opisuje trenutno stanje igrača, primena State šablona. 

```csharp
public abstract class IPlayerState
{
    public IAction selectedAction;  				// Koju akciju trenutno igrač priprema/izvršava
    public abstract void Update(Vector3 mouseWorldPosition);	// Update koji cemo zvati svakog frejma, prima trenutnu poziciju miša


    public abstract void Enter();				// Metode za prelaze kroz stanja
    public abstract void Exit();

    public abstract Task<bool> Execute();			// Metoda koja ce izvrsiti trenutno odabranu akciju
}
```

Igrač se može naći u ukupno 3 stanja: 
- DefaultTurnState - default stanje kada je igrač na potezu
- TargetingState - stanje u koje igrač ulazi kada odabere akciju koju želi da izvrši
- WaitingForTurnState - stanje u kojem se igrač nalazi dok čeka na svoj potez

### Targeting State

```csharp
public class TargetingState : IPlayerState
{

    public TargetingState(IAction action) // Konstruktor stanja, pri instanciranju novog TargetingState-a navodimo koju to akciju trenutno ciljamo
    {
        this.selectedAction = action;
    }

    public override async Task<bool> Execute()	// Izvršava trenutnu akciju
    {
        bool finished = await selectedAction.Execute();
        return finished;
    }

    public override void Update(Vector3 mouseWorldPosition) // Ažurira ActionContext trenutne akcije, a potom ažurira UI na osnovu toga.
    { 
        Vector3Int targetedTile = GridEntitiesManager.instance.GetCellFromPosition(mouseWorldPosition);
        ActionContext newContext = new ActionContext();
        newContext.targetedTile = targetedTile;

        if (!selectedAction.resolving)
        {
            bool contextUpdated = selectedAction.UpdateContext(newContext);
            selectedAction.RedrawTiles();
        }
    }

    public override void Enter()	// Pri ulasku u akciju, cisti UI table, i Invoke-uje event koji sadrži trenutnu akciju za potrebe UI-a
    {
        PlayerManager.instance.playerCharacter.CharacterActionInitiated(selectedAction);
        SelectedTilesManager.instance.ClearRangeTiles();
        SelectedTilesManager.instance.ClearTargetingTiles();
        selectedAction.DrawTiles();
    }

    public override void Exit()		//Pri izlasku iz stanja, ocisti UI za sobom
    {
        SelectedTilesManager.instance.UnLockHighlights();
        SelectedTilesManager.instance.ClearRangeTiles();
        SelectedTilesManager.instance.ClearTargetingTiles();
    }
}
```

TargetingState je najbitniji od sva 3 stanja. On predstavlja mešavinu State i Strategy šablona. Iz stanja pripreme jedne akcije, možemo preći u stanje pripreme druge akcije,
bez da pisemo stanja za svaku akciju. Svaka akcija je odgovorna da definiše svoje ponašanje, i kako treba da se ažurira UI, TargetingState se oslanja na to.
DefaultTurnState je samo TargetingState u kom je akcija "zakucana" na akciju kretanja, osim ako se ne hoveruje preko neprijatelja, kada je zakucana na akciju obicnog napada.
WaitingForTurnState samo implementira sve metode apstraktne klase IState, ali u implementaciji ne stoji nikakav kod, jer igrač ni ne može ništa da radi dok čeka svoj potez,
sem da gleda UI i pomera kameru i sl.

### PlayerManager

PlayerManager je singleton klasa zaslužna je za čuvanje trenutnog stanja, kao i interakciju sa korisničkim input-om.
Nasledjuje MonoBehaviour abstraktnu klasu, koja je ugradjena klasa u Unity koja omogućava da se skripta zakači na GameObject.


```csharp
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public IPlayerState currentState;
    public PlayerCharacter playerCharacter;
    [SerializeField] public LayerMask npcLayerMask;
    public List<ActionHolder> availableActions = new();
    public List<ActionData> allActionsData = new();
    public int currentActionIndex;

    void Awake()		// Postavljanje instance, Unity poziva Awake pri učitavanju skripte
    {
        instance = this;
    }

    void Start()		// Postavljanje početnog stanja na WaitingForTurnState. Unity poziva Start samo jednom, na prvom frejmu igrice. 
    {
        currentState = new WaitingForTurnState();
        currentState.Enter();
        PlayerCharacter.OnStartPlayerTurn += StartTurn; // Pretplaćujemo se na event koji označava da je igračev potez na redu, kada se event desi, zvaće se StartTurn funkcija dole
    }

    void Update() 		// Unity poziva Update funkciju jednom po frejmu. Trenutna pozicija cursora se prebacuje u koordinatni sistem igrice i šalje se trenutnom stanju.
    {
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        currentState.Update(worldPosition);
    }

    void ResetState()		// Restartovanje stanja na default stanje
    {
	currentState.Exit();
        currentActionIndex = 0;
        currentState = new DefaultTurnState();
        currentState.Enter();
    }

    public void Click(InputAction.CallbackContext context) // Ova funkcija se izvrašava na pritisak levog klika miša. Ukoliko nije miniigra aktivna, zovemo Execute metodu.
    {
        if (context.performed)
        {
            if (MinigameManager.instance.isActive)
            {
                MinigameManager.instance.Hit();
            }
            else
            {
                _ = Execute();
            }
        }
    }

    public async Task Execute()		// Poručuje trenutnom stanju da izvrši njegovu akciju. Ukoliko se akcija uspešno izvršila, vratiće True, te je onda potrebno restartovati stanje
    {
        bool finished = await currentState.Execute();
        if (finished)
        {
            playerCharacter.CharacterActed(currentState.selectedAction);
            availableActions[currentActionIndex].SetCooldown(currentState.selectedAction.cooldown);
            currentState.Exit();
            ResetState();
        }
    }

    public void Cancel(InputAction.CallbackContext context)	// Ukoliko odlučimo da otkažemo akciju koju trenutno spremamo, desnim klikom se vracamo na default akciju, ako je nas potez
    {
        if (context.performed && currentState is not WaitingForTurnState && !currentState.selectedAction.resolving)
        {
            currentState.Exit();
            ResetState();
        }
    }

    public void EndTurn()	// Funkcija se poziva prilikom pritiska dugmeta za završavanje poteza
    {
        if (TurnManager.instance.currentTurn == playerCharacter)
        {
            playerCharacter.EndTurn();
            currentState.Exit();
            currentState = new WaitingForTurnState();
            currentState.Enter();
        }
    }

    public void StartTurn()	// Funkcija koja se poziva kada igračev potez počinje
    {
        currentState.Exit();
        ResetState();
    }

    
    public void useAbility(int index)	// Na osnovu ui-a ili tastera na tastaturi, pripremamo našu akciju i ulazimo u stanje ciljanja, odnosno TargetingState te akcije
    {
        if (currentState is not WaitingForTurnState && availableActions.Count > index)
        {
            bool created = availableActions[index].TryCreateAction(out IAction action);

            if (created)
            {
                currentActionIndex = index;
                currentState.Exit();
                currentState = new TargetingState(action);
                currentState.Enter();
            }
        }
    }
}
```


### Perks

Perkovi su trajni bonusi koji osnažuju igrača, i menjaju način na koji pristupa potezima.
Svaki drukcije utiče na igrača i njegove akcije, uglavnom se služe hookovima koji se nalaze u akcijama.
Svaki perk implementira apstraktnu klasu Perk, koja samo definise sta sve treba da implementira.

```csharp
public abstract class Perk
{
    public Character owner;
    public Character relatedCharacter;

    public Perk(Character character) // Konstruktor, označavamo čiji i je perk, i opciono da li perk ima veze sa još nekim karakterom
    {
        this.owner = character;	
        Initialize();
    }

    public abstract void Initialize();	// Aktiviranje perka, obično u vidu subscribeovanja na neki event

    public abstract void OnRemove();	// Deaktiviranje perka, obično u vidu unsubscribeovanja na neki event

    public void Remove()
    {
        OnRemove();
        owner.activePerks.Remove(this);
    }

}
```

Primer perka KillRushPerk- Whenever you kill an enemy, restore 2 AP.

```csharp
public class KillRushPerk : Perk
{
    int APGain;
    public KillRushPerk(Character character, int APGain) : base(character)
    {
        this.APGain = APGain;
    }

    public override void Initialize()
    {
        owner.OnCharacterKilledEnemy += RefreshAP;
    }

    private void RefreshAP()
    {
        owner.ChangeAP(+APGain);
    }

    public override void OnRemove()
    {
        owner.OnCharacterKilledEnemy -= RefreshAP;
    }
}
```

### Status Effects

Statusni efekti su slični perkovima, samo što mogu uticati na bilo koga.
Ključna razlika je da su statusni efekti kratkotrajni, mogu da isteknu posle odredjenog broja poteza, ili kada se ispuni neki uslov.
Mogu biti pozitivni (buff) i negativni (debuff) po karaktera koji ih ima.
Svi statusni efekti nasledjuju baznu klasu StatusEffect

```csharp
public enum StatusEffectName { STUN, ROOT, CHAINED, DISARMED, CURSE, DOT, HOT, ENGINEOFF_STUN, ENGINEOFF_PENALTY };
public enum StatusEffectType { BUFF, DEBUFF };
public abstract class StatusEffect
{
    public StatusEffectName name;
    public StatusEffectType type;
    public int duration;	        // Dužina trajanja statusa izražena u potezima
    public Character owner;	        // Vlasnik statusa, na koga to utiče  
    public Character relatedCharacter;  // Statusni efekti su kratkotrajni, te mogu biti povezani sa još nekim karakterom

    public StatusEffect(int duration, Character character, Character relatedCharacter = null) 
    {											    
        this.duration = duration;	
        this.owner = character;
        this.relatedCharacter = relatedCharacter;
        Initialize();
    }

    public void DecreaseDuration()
    {
        this.duration -= 1;
        if (this.duration <= 0)
        {
            Remove();
        }
    }

    public abstract void Initialize();

    public abstract void OnRemove();

    public void Remove()
    {
        OnRemove();
        owner.activeEffects.Remove(this);
    }

}
```

Primer status efekta StunEffect - Negativni status koji načini da karakter preskoči svoj potez

```csharp
public class StunEffect : StatusEffect
{
    public StunEffect(int duration, Character character) : base(duration, character)
    {
        this.name = StatusEffectName.STUN;
        this.type = StatusEffectType.DEBUFF;
    }

    public override void Initialize()
    {
        owner.OnCharacterStartTurn += owner.CantAct;
        owner.OnCharacterEndTurn += DecreaseDuration;
    }

    public override void OnRemove()
    {
        owner.OnCharacterStartTurn -= owner.CantAct;
        owner.OnCharacterEndTurn -= DecreaseDuration;
    }
}
```

### Napomena

Sve do sada je u potpunosti već implementirano. Neke stvari poput UI logike i logike heksagonalne table sam izostavio jer nisu preterano zanimljivi.
Dalje navedene stvari su ili delimično ili nisu uopšte implementirane ali su u planu, sortirano po prioritetu.

### Enemies

Postojalo bi više vrste neprijatelja, i svaki bi imao sopstvenu logiku.
To bi bio neki strategy, gde svaki neprijatelj ima svoju logiku koju izvršava neki EnemyManager, samo zove enemyLogic.Act(), gde možemo menjati enemyLogic u zavisnosti od neprijatelja,
a ostatak koda da ostane isti.
Takodje, svaki put kada igrač izroni i vrati se nazad, neprijatelji postaju čudovišnija verzija sebe, i to tako do 3 puta, te ćemo za svakog neprijatelja imati po 3 verzije.
Na primeru meduze:

Ako imamo sobu punu meduza, u zavisnosti od toga koliko puta je igrac do sada izranjao, treba da stvorimo drukciju meduzu, te bismo tu imali neki EnemyAbstractFactory, koji bi
instancirao nove neprijatelje, u zavisnosti od toga koji je to neprijatelj koji nam treba, i koliko izopačen on treba da bude (više familija istih klasa) 

### Ostalo

UI za perkove i status efekte (funkcionisu ali fali ui element da se zna da su aktivni)
XP sistem preko kog bi se otkljucavale nove akcije i perk-ovi.
Istraživanje po sobama.





