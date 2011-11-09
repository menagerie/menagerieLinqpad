<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\WPF\UIAutomationTypes.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\UIAutomationClient.dll</Reference>
  <Namespace>System.Windows.Automation</Namespace>
</Query>

private IMenagerieCommandParser menagerieCommandParser;
private IMenagerieCommandParser MyMenagerieCommandParser
{ 
	get
	{
		return menagerieCommandParser ?? (menagerieCommandParser = new MenagerieCommandParser());
	}
	set
	{
		menagerieCommandParser = value;
	}
}


void Main()
{
	Menagerie myMenagerie = new Menagerie();
	
	string input;
	while ((input = Console.ReadLine()) != string.Empty)
	{
		IMenagerieCommand command = MyMenagerieCommandParser.CreateCommand(myMenagerie, input);
		if (command.Valid)
		{
			command.Execute();
		}
		else
		{
			Console.WriteLine("Unrecognised command");
		}
	}
}

public void RemoveAnimal(string[] tokens, Menagerie myMenagerie)
{

}

public interface IMenagerieCommand
{
	bool Valid { get; }
	void Execute();
}

public class InvalidCommand : IMenagerieCommand
{
	public bool Valid { get { return false; } }
	public void Execute()
	{
		throw new ApplicationException("Can't execute an invalid command!");
	}
}

public class AddAnimalCommand : IMenagerieCommand
{
	public string name;
	public IMenagerie menagerie;
	public IAnimalFactory factory;
	
	public AddAnimalCommand(IMenagerie menagerie, string animalName, IAnimalFactory factory)
	{
		this.menagerie = menagerie;
		this.name = animalName;
		this.factory = factory;
	}
	
	public bool Valid { get { return true; } }
	
	public void Execute()
	{
		menagerie.AddAnimal(factory.Factory(name));
	}
}

public class FeedAnimalCommand : IMenagerieCommand
{
	public string name;
	public string food;
	public IMenagerie menagerie;
	
	public FeedAnimalCommand(IMenagerie menagerie, string name, string food)
	{
		this.menagerie = menagerie;
		this.name = name;
		this.food = food;
	}
	
	public bool Valid { get { return true; } }
	
	public void Execute()
	{
		IAnimal animal = menagerie.FindAnimal(name);
		if (animal != null)
		{
			animal.Feed(food);
		}
	}
}

enum CommandType
{
	Add,
	Feed,
	Pet
}

public interface IMenagerieCommandParser
{
	IMenagerieCommand CreateCommand(IMenagerie menagerie, string input);
}
public class MenagerieCommandParser : IMenagerieCommandParser
{	
	IDictionary<string, IAnimalFactory> animalFactoryLocator //locator anti-pattern, should use DI framework
		= new Dictionary<string, IAnimalFactory>
	{
		{"cat", new CatFactory()},
		{"dog", new DogFactory()}
	};
	
	public IMenagerieCommand CreateCommand(IMenagerie menagerie, string input)
	{
		var tokens = input.Split(new [] { ' ' });
		var command = tokens[0];
		var animal = tokens[1];
		var name = tokens[2];
		
		CommandType commandType;
		if (!Enum.TryParse(command, true, out commandType))
		{
			return new InvalidCommand();
		}
		
		switch (commandType)
		{
			case CommandType.Add:
				return CreateAddCommand(menagerie, animal, name);
			case CommandType.Feed:
				return new FeedAnimalCommand(menagerie, animal, name);
			default:
				return new InvalidCommand();
		}
	}
	
	public IMenagerieCommand CreateAddCommand(IMenagerie menagerie, string animal, string name)
	{
		IAnimalFactory factory = animalFactoryLocator[animal];
		if (factory != null)
		{
			return new AddAnimalCommand(menagerie, name, factory);
		}
		else 
		{
			return new InvalidCommand();
		}
	}
}

public interface IAnimalFactory
{
	IAnimal Factory(string name);
}

public class CatFactory : IAnimalFactory
{
	public IAnimal Factory(string name)
	{
		return new Cat(name, "Chilli Doritos");
	}
}

public class DogFactory : IAnimalFactory
{
	public IAnimal Factory(string name)
	{
		return new Dog(name, "Baked beans on toast");
	}
}

public interface IMenagerie
{
	void AddAnimal(IAnimal a);
	IAnimal FindAnimal(string name);
}

public class Menagerie : IMenagerie
{
	private List<IAnimal> animals = new List<IAnimal>();
	
	public void AddAnimal(IAnimal a)
	{
		Console.WriteLine(string.Format("A new {0} called {1} has entered the menagerie", 
										a.GetType().Name.ToLower(), a.Name));
		animals.Add(a);
	}
	
	public IAnimal FindAnimal(string name)
	{
		return animals.FirstOrDefault(a => a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
	}
}

public interface IAnimal
{
	string Name { get; set; }
	void MakeNoise();
	void EatStuff();
	void RunAbout();
	void Feed(string food);
}

public abstract class Animal : IAnimal
{
	private System.Timers.Timer timer = new System.Timers.Timer();
	
	public Animal()
	{
		timer.Interval = 5000;
		timer.Elapsed += DoRandomThing;
		timer.Start();
	}
	
	public string Name { get; set; }
	public abstract void MakeNoise();
	public abstract void EatStuff();
	public abstract void RunAbout();
	public abstract void Feed(string food);
	
	private void DoRandomThing(object sender, System.Timers.ElapsedEventArgs e)
	{
		switch(new Random().Next(10))
		{
			case 0:
				MakeNoise(); break;
			case 1:
				EatStuff(); break;
			case 2:
				RunAbout(); break;
			default:
				break;
		}
		
	}
}

public class Cat : Animal
{	
	public string FavouriteFood  { get; set; }
	
	public Cat(string name, string favouriteFood)
	{
		this.Name = name;
		this.FavouriteFood = favouriteFood;
	}
	public override void MakeNoise()
	{
		Console.WriteLine(string.Format("{0} went meow", Name));
	}
	public override void EatStuff()
	{
		Console.WriteLine(string.Format("{0} ate some delicious {1}", Name, FavouriteFood));
	}
	
	public override void RunAbout()
	{
		Console.WriteLine(string.Format("{0} is stalking through the undergrowth", Name));
	}
	
	public override void Feed(string food)
	{
		Console.WriteLine(string.Format("{0} is munching on some tasty {1}", Name, food));
	}
}

public class Dog : Animal
{	
	public string FavouriteFood  { get; set; }
	
	public Dog(string name, string favouriteFood)
	{
		this.Name = name;
		this.FavouriteFood = favouriteFood;
	}
	public override void MakeNoise()
	{
		Console.WriteLine(string.Format("{0} went woof", Name));
	}
	public override void EatStuff()
	{
		Console.WriteLine(string.Format("{0} ate some delicious {1}", Name, FavouriteFood));
	}
	
	public override void RunAbout()
	{
		Console.WriteLine(string.Format("{0} is chasing its tail", Name));
	}
	
	public override void Feed(string food)
	{
		Console.WriteLine(string.Format("{0} is gnawing on some tasty {1}", Name, food));
	}
}