using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace SausageFestModNS
{
    public class SausageFestMod : Mod
    {
        public override void Ready()
        {
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "sausagefestmod_blueprint_slaughterhouse", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "sausagefestmod_blueprint_meat_grinder", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_cooked_sausage", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_cooked_blood_sausage", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_raw_sausage", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_raw_blood_sausage", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_blutwurst_beer", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_wurst_beer", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_potion_healing", 1);
			
			Logger.Log("Thorrific's Sausage Fest is now ready! Bon appetit ;)");
        }
    }

	/*
	* Had to rename SlaughterHouse into SausageFestSlaugtherhous to make a distinction with the default Stacklands 'Butchery' (which is actually called SlaughterHouse already in the Stacklands implementation...)
	*/
	public class SausageFestSlaughterhouse : SlaughterHouse
	{
		public override bool DetermineCanHaveCardsWhenIsRoot => true;
				
		public override bool CanHaveCardsWhileHasStatus()
		{
			return true;
		}

		// this method decides whether a card should stack onto this one
		protected override bool CanHaveCard(CardData otherCard)
		{
			int num = GetChildCount() + (1 + otherCard.GetChildCount());
			if (otherCard.Id == "cow")
			{
				return num <= 5;
			}
			return false;
		}

		// this method is called every frame, it is the CardData equivalent of the Update method
		public override void UpdateCard()
		{
			if (MyGameCard.HasChild && MyGameCard.Child.CardData is Animal)
			{
				MyGameCard.StartTimer(30f, SlaughteringAnimal, SokLoc.Translate("sausagefestmod_action_slaughtering_status"), GetActionId("SlaughteringAnimal"));
			}
			else
			{
				MyGameCard.CancelTimer(GetActionId("SlaughteringAnimal"));
			}
			//base.UpdateCard();
		}

		[TimedAction("slaughtering_animal")]
		public void SlaughteringAnimal()
		{
			if (MyGameCard.HasChild && MyGameCard.Child.CardData is Animal)
			{
				GameCard child = MyGameCard.Child;
				RemoveFirstChildFromStack();
				child.DestroyCard();
				// Raw meat
				CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "raw_meat", checkAddToStack: false);
				WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
				// Organ meat
				cardData = WorldManager.instance.CreateCard(base.transform.position, "sausagefestmod_organ_meat", checkAddToStack: false);
				WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir); // required to make organ meat auto-stack if there is a card in the vicinity.
				// Blood
				cardData = WorldManager.instance.CreateCard(base.transform.position, "sausagefestmod_blood", checkAddToStack: false);
				WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir); // required to make blood auto-stack if there is a card in the vicinity.

				WorldManager.instance.CreateSmoke(base.transform.position);
			}
		}
	}

	public class SausageFestMeatGrinder : CardData
	{
		// this method decides whether a card should stack onto this one
		protected override bool CanHaveCard(CardData otherCard)
		{
			if (otherCard.Id != "sausagefestmod_organ_meat" && otherCard.Id != "sausagefestmod_blood")
				return false;
			return true;
		}
		
		// this method is called every frame, it is the CardData equivalent of the Update method
		public override void UpdateCard()
		{
			base.UpdateCard();
		} 

		public override bool CanHaveCardsWhileHasStatus()
		{
			return true;
		}
	}

	/*
	* Delirium tremens (DTs) is a severe form of alcohol withdrawal (existing in real life)
	* that typically occurs in people who have been drinking heavily for an extended period 
	* and then suddenly stop or significantly reduce their alcohol intake.
	*
	* It is characterized by sudden and severe mental or nervous system changes.
	* It is sometimes represented as seeing pink elephants.
	*
	* The "Delirium Tremes Wild" is a conceptual representation of the above:
	* 1. It can spawn by having a villager drink (blood) sausage beer (5% chance)
	* 2. It is tamed by drinking "sausage beer" (changing it into a DeliriumTremesTamed card)
	* 3. It is frenzied by drinking "blood sausage beer"
	*
	* It's represented as two different creatures below (wild-mob & tamed-villager)
	*/
	
	/*
	* Create a class called DeliriumTremensWild which extends the Mob class --> wild (unfriendly) version
	* It has chance to spawn, each time when a villager sobers up.
	* It can be taimed by providing it a wurst beer.
	*/
	public class DeliriumTremensWild : Mob
	{
		public override void OnInitialCreate()
		{
			HealthPoints = 150;
			base.OnInitialCreate();
		}
		
		// this method decides whether a card should stack onto this one
		protected override bool CanHaveCard(CardData otherCard)
		{
			if (otherCard is SausageFestWurstBeer)
			{
				return true;
			}
			return false;
		}
		
		// this method is called every frame, it is the CardData equivalent of the Update method
		public override void UpdateCard()
		{
			if (MyGameCard.GetLeafCard().CardData is SausageFestWurstBeer)
			{
				MyGameCard.StartTimer(5f, DrinkingWurstBeer, SokLoc.Translate("sausagefestmod_action_drinking_wurstbeer_status"), GetActionId("DrinkingWurstBeer"));
			}
			else
			{
				MyGameCard.CancelTimer(GetActionId("DrinkingWurstBeer"));
			}
			base.UpdateCard();
		}

		[TimedAction("drinking_beer")]
		public void DrinkingWurstBeer()
		{
			CardData child = MyGameCard.GetLeafCard().CardData;
			if (child is SausageFestWurstBeer) {
				((SausageFestWurstBeer)child).ConsumedBy(this);
				child.MyGameCard.DestroyCard(); // destroy consumed beer-card
			}
		}

		/*
		* Helper method to make a wild delirium tremes change into a tame version.
		* It will also become Frenzy as a result of it.
		*/
		public static void MakeTame(Combatable vill)
		{
			if (vill.Id == "sausagefestmod_delirium_tremens_wild")
			{
				CardData cardData = WorldManager.instance.ChangeToCard(vill.MyGameCard, "sausagefestmod_delirium_tremens_tamed");
				WorldManager.instance.CreateSmoke(cardData.transform.position);
				cardData.MyGameCard.SendIt();
			}
		}

		
		// Helper method to create a new wild delirium tremes (5% chance)
		public static void CreateIfUnlucky(Vector3 position)
		{
			if (UnityEngine.Random.Range(0, 100) < 5)
			{
				CardData cardData = WorldManager.instance.CreateCard(position, "sausagefestmod_delirium_tremens_wild", checkAddToStack: false);
				cardData.MyGameCard.SendIt();
			}
		}
	}

	/*
	* Create a class called DeliriumTremensTamed which extends the Mob class --> friendly (drunk) version
	* It spawns when the aggro version is fed the right type of alcohol.
	*/
	public class DeliriumTremensTamed : Villager
	{
		// this method decides whether a card should stack onto this one
		protected override bool CanHaveCard(CardData otherCard)
		{
			if (otherCard.Id == "sausagefestmod_wurst_beer" || otherCard.Id == "sausagefestmod_blutwurst_beer" || otherCard.Id == "sausagefestmod_potion_healing")
			{
				return true;
			}
			else return false; //return base.CanHaveCard(otherCard); // otherwise, we will let Animal.CanHaveCard decide
		}
		
		public override bool HasInventory  => false;
		
		// this method is called every frame, it is the CardData equivalent of the Update method
		public override void UpdateCard()
		{
			if (MyGameCard.GetLeafCard().CardData is SausageFestWurstBeer)
			{
				MyGameCard.StartTimer(5f, DrinkingWurstBeer, SokLoc.Translate("sausagefestmod_action_drinking_wurstbeer_status"), GetActionId("DrinkingWurstBeer"));
			}
			else
			{
				MyGameCard.CancelTimer(GetActionId("DrinkingWurstBeer"));
			}
			base.UpdateCard();
		}

		[TimedAction("drinking_beer")]
		public void DrinkingWurstBeer()
		{
			CardData child = MyGameCard.GetLeafCard().CardData;
			if (child is SausageFestWurstBeer) {
				((SausageFestWurstBeer)child).ConsumedBy(this);
				child.MyGameCard.DestroyCard(); // destroy consumed beer-card
			}
		}

		/*
		* Helper method to make a tamed delirium tremes change into a wild version.
		* It will also become Frenzy as a result of it.
		*/
		public static void MakeWild(Combatable vill)
		{
			if (vill.Id == "sausagefestmod_delirium_tremens_tamed")
			{
				CardData cardData = WorldManager.instance.ChangeToCard(vill.MyGameCard, "sausagefestmod_delirium_tremens_wild");
				WorldManager.instance.CreateSmoke(cardData.transform.position);
				cardData.MyGameCard.SendIt();
			}
		}
	}


	/*
	* The (blut) wurst beer can be put:
	* - either on a villager, that will drink the beer, and when consumed potentially spawns delirium tremes mob.
	* - either on the delrium tremes mob itself, that will drink the beer
	* - - if delerium tremes consumes a regular "beer wurst", then there is a chance that it becomes friendly.
	* - - if delirum tremes consumes a blood "beer wurst", then it becomes hostile.
	*/
	public class SausageFestWurstBeer : Food
	{
		public override void OnInitialCreate()
		{
			base.OnInitialCreate();
			
			ResultAction = "add:StatusEffect_Drunk";
			if (Id == "sausagefestmod_blutwurst_beer") {
				ResultAction += ";" + "add:StatusEffect_Frenzy";
			}
		}

		public override void UpdateCard()
		{
			if (MyGameCard.HasParent)
			{
				if (MyGameCard.Parent.CardData is Combatable && MyGameCard.Parent.CardData is not DeliriumTremensTamed && MyGameCard.Parent.CardData is not DeliriumTremensWild)
				{
					MyGameCard.StartTimer(5f, DrinkingWurstBeer, SokLoc.Translate("sausagefestmod_action_drinking_wurstbeer_status"), GetActionId("DrinkingWurstBeer"));
				}
			}
			else
			{
				MyGameCard.CancelTimer(GetActionId("DrinkingWurstBeer"));
			}
			base.UpdateCard();
		}

		[TimedAction("drinking_beer")]
		public void DrinkingWurstBeer()
		{
			if (MyGameCard.HasParent && MyGameCard.Parent.CardData is Combatable)
			{
				ConsumedBy((Combatable)MyGameCard.Parent.CardData);
				MyGameCard.Child.DestroyCard(); // destroy consumed beer card
			}
		}


		// If the current food consumed is a "Blood Sausage Beer" && it is consumed by a Tamed Delirium Tremens --> then make it wild.
		public override void ConsumedBy(Combatable vill)
		{
			base.ConsumedBy(vill);
			//AudioManager.me.PlaySound(AudioManager.me.Eat, Position); 
			
			if (vill.Id == "sausagefestmod_delirium_tremens_tamed") {
				if (Id == "sausagefestmod_blutwurst_beer") { // If tamed-version consumed blutwurst-beer
					DeliriumTremensTamed.MakeWild(vill); // Make wild
				}
			}
			else if (vill.Id == "sausagefestmod_delirium_tremens_wild") {
				if (Id == "sausagefestmod_wurst_beer") {  // If wild-version consumed regular wurst-beer
					DeliriumTremensWild.MakeTame(vill); // Make tame
				}
			}
			else // otherwise something else, with 5% chance to trigger a new Delirium Tremes mob
			{
				DeliriumTremensWild.CreateIfUnlucky(base.transform.position);
			}
		}
	}

	public class SausageFestPotionHealing : Food
	{
		public override void UpdateCard()
		{
			if (MyGameCard.HasParent && MyGameCard.Parent.CardData is Combatable)
			{
				Combatable parentCardData = (Combatable)MyGameCard.Parent.CardData;
					//MyGameCard.StartTimer(5f, DrinkingWurstBeer, SokLoc.Translate("sausagefestmod_action_drinking_wurstbeer_status"), GetActionId("DrinkingWurstBeer"));
					//MyGameCard.Parent.CardData

				// Only consume potion in case healing is required
				if (parentCardData.HealthPoints < parentCardData.RealBaseCombatStats.MaxHealth)
				{
					var healthPotions = parentCardData.ChildrenMatchingPredicate(childCard => childCard.Id == "sausagefestmod_potion_healing");
					if (healthPotions.Count > 0) // if there are any health potions on the combatable
					{
						int healed = 0; // create a variable to keep track of how much health gained
						foreach (CardData healthPotion in healthPotions) // for each health potion
						{
							if (parentCardData.HealthPoints < parentCardData.RealBaseCombatStats.MaxHealth) // if healing still required
							{
								var healingPoints = Mathf.Min(parentCardData.RealBaseCombatStats.MaxHealth - parentCardData.HealthPoints, 2);
								
								parentCardData.HealthPoints += healingPoints; // increase health by 2
								healed += healingPoints;
								healthPotion.MyGameCard.DestroyCard(); // destroy health potion
							}
							else break;
						}
						AudioManager.me.PlaySound(AudioManager.me.Eat, parentCardData.Position); // play the eating sound
						WorldManager.instance.CreateSmoke(Position); // create smoke particles
						parentCardData.CreateHitText($"+{healed}", PrefabManager.instance.HealHitText); // create a heal text that displays how much it healed in total
						parentCardData.UpdateCard();
					}
				}
			}
		}
	}


/*
	public class VillagerDwarfScout : Villager
	{
		public override void OnInitialCreate()
		{
			HealthPoints = 15;
			base.OnInitialCreate();
		}
	}
	*/
}