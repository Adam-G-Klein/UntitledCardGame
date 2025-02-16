using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EndScreenUI : MonoBehaviour
{ 
    private static string AidenDesc = "Aiden was as the only son of three children in an insular, middle-class, Irish-American, Catholic-practicing family. He met his future wife, April, in university and she inspired him to follow his dreams and break out of his shell. They kept in touch well after graduation, eventually getting married and moving to the suburbs. Unfortunately, the marriage was quietly unhappy, stemming from problems that were rarely addressed aloud. Aiden was too focused on his work and April, having quit her photojournalist aspirations to be a housewife, began to retreat into herself. \n\nAs Aiden neared retirement age and began to consider how to make up for time lost with his wife, April received a diagnosis for a terminal illness and passed away much sooner than expected. After her death, Aiden found an unfinished but emotionally charged letter from April, which exacerbated his regrets. Grief-stricken, he embarked on a journey of self reflection which led him to the Estate. ";
    private static string AborahDesc = "Aborah and her twin brother grew up just as tensions were rising with foreign colonizing powers in the Asante empire. While her brother joined the ranks of warriors preparing to move against the British, Aborah pledged her life to the protection of Yaa Asantewaa, the Queen Mother. Aborah was an unparalleled warrior, trained from years of secretly beating her brother at his own combat practice. Unfortunately, much to Aborah's great shame, she could not stop the British from capturing the Queen Mother and all her advisors. Her final orders were to flee with the golden stool, the symbol of her people, their power, and their history, until the foreign soldiers gave up looking for it. \n\nWhile in hiding, Aborah chanced upon her twin brother, now a prisoner paraded around by the British.She had a crisis of what was more dear to her–in the chaos, Aborah made a decision and kicked off events so traumatic she doesn't recall what happened. She woke up in the Estate, not sure how she got there but determined to retrieve what was lost to her, even if she can't remember which one it was that she gave up in the first place.";
    private static string JeanDesc = "Jean is a woman of many layers and sometimes she's not even sure which part is true anymore. Of Bengali and British descent, Jean was brought to London as a child but kicked out of her mother's employer's home and left to fend for herself. She picked up friends and skills on the street, lost the friends but honed the skills, and made a name for herself as a fortune teller and magician of ambiguous origin. Always on the run from the authorities, Jean eventually takes her hard earned money from conning the wealthy and heads to America to both lie low and charm the pants off the social elite of the East Coast. \n\nSomehow, in a moment that was supposed to be her big triumph, she ends up trapped in the Estate instead. \n\nThe Diamond of the East? Where in the East? Nobody knows.";
    private static string ReneDesc = "René was a larger than life person from birth. Born at a whopping 11 pounds, he quickly learned to fight external preconceptions with pizazz and charisma, enchanting bullies and admirers alike. René went from entertaining in the schoolyard to the vaudeville circuit all the way to the silver screen. He had a natural comedic instinct that made him a silent film star, but he was often typecast into roles deemed appropriate for an \"ambiguously brown\" actor. \n\nUltimately, his firm rejection of certain roles and wish to be involved in the creative process did not endear him to the studios.When his name was brought up as part of a typical industry scandal, the media, perhaps goaded on in secret by the studios, decimated his career by villainizing him with no proof.René calmly denied every accusation but the public turned against him and he was blacklisted from the industry.It took many years but with the help of a few loyal friends, he made his return to film and was cast in his first speaking role. \n\nShortly after filming, he attended his first industry party in years at the behest of his friends, arriving at the Estate in good spirits but never emerging again.";
    private static string ArchDesc = "Cordial but not friendly, reserved but not cold, sympathetic but not emotional, the Architect is an enigma to his companions. In reality, \"The Architect\" is a misnomer–in his life, he was actually an engineer, albeit with design-minded tendencies. Completely dominated by his obligations and the need to please his family, the Architect put his head down and dutifully did his job only to become horribly depressed and deeply unsatisfied with his life. Breaking under the pressure to live life according to his family's standards, he starts a life of very mild crime and then fully runs away from his lucrative job and family out of guilt and internal conflict. It is in this morally grey state that he ends up in the middle of winter on the steps of the Estate. Invited inside to keep warm from the growing storm, the Architect agrees to stay the night and then never exits the Estate again.";
    private static string EntropyDesc = "Entropy comes from a future where artificial intelligence was used by humans to wage war against each other. In the process of gaining sentience, the AI destroyed most of humanity instead, but a mistake in the code led Entropy, one of the many hive mind unmanned killers under the AI's control, to become independent in thought and action. Just as Entropy began to become capable of regret and appreciation of life, the last of the humans pulled off their final plan: a powerful space-time bomb that would wipe and refresh the world. Entropy was caught up in the blast and hurled through time into the Estate via a rift in reality. It is unclear if their timeline is the same as Aiden's.";
    private static string ConciergeDesc = "Before he became the Concierge, Lawrence Roth/de Wrotham/Wreath was a middle class British man trying so hard to integrate himself into the upper class that he went into debt and ended up in prison for it. Undeterred, while imprisoned, he polished his skills at talking people into doing anything he wanted. Lawrence then used those skills to not only get back out of prison but to go to America and get a job at the Estate. To be fair, Lawrence wasn't all talk—he single-handedly revived the Estate into a place where celebrities and great minds alike gathered to mingle and party. It was his hard work that led him to meeting noted scientist, Maria Seele, and providing her the resources to accidentally create a cosmic wormhole that tore space and time apart.";
    private static string EstateDesc = "The Estate is said to have been built on a place of power, somewhere where primordial lightning struck more than once, where trees grew taller and thicker, where creatures gathered to meet with the seasons, where natural and artificial harvests yielded great bounty. \n\nThe building itself has had a shifting exterior and function throughout history.Known to outsiders sometimes as a country club, sometimes as an orphanage, sometimes as a museum, sometimes as a boarding house, sometimes as a designated historical structure, sometimes as a stash site, sometimes as a haunted lot, sometimes as a salon, sometimes as the hottest spot in town… the Estate spends most of its time as a nondescript building to the average person but appears like a beacon to the truly desperate.\n\nThose who manage to stumble across the Estate and still exit again whole have only vague recollections of the interior, hazy memories of grand furnishings and low lighting and the distinct feeling of entering a space that felt out of time.";
    private VisualElement root;
    [SerializeField]
    private bool displayOnStart = true;
    private Dictionary<string, string> titles = new Dictionary<string, string> {
    { "Aiden", "Aiden J. Dyer" },
    { "Aborah", "Aborah" },
    { "Jean", "Jean" },
    { "Rene", "René" },
    { "Architect", "The Architect"},
    { "Entropy", "Entropy (NTRO-P1)"},
    { "Concierge", "The Concierge"},
    { "Estate", "The Estate"}
};
    private Dictionary<string, string> flavor = new Dictionary<string, string> {
    { "Aiden", "The Protagonist" },
    { "Aborah", "Adusa Adwoa Ataá Afiríyie Aborah -- The Fighter" },
    { "Jean", "Miss Jean Adamantis, The Inscrutable Diamond of the East -- The Seer" },
    { "Rene", "Pablo Renato \"René\" Delgado de la Torre -- The Clown" },
    { "Architect", "[redacted] -- The Strategist"},
    { "Entropy", "Nonorganic Teleprocessing Remote Operative - Pollistes 1 -- The Robot"},
    { "Concierge", "The Mentor. The Villain. -- That annoying guy that shows up everywhere."},
    { "Estate", ""}
};

    private Dictionary<string, string> desc = new Dictionary<string, string> {
    { "Aiden", AidenDesc },
    { "Aborah", AborahDesc },
    { "Jean", JeanDesc },
    { "Rene", ReneDesc },
    { "Architect", ArchDesc },
    { "Entropy", EntropyDesc},
    { "Concierge", ConciergeDesc},
    { "Estate", EstateDesc}
};
    private Dictionary<string, string> images = new Dictionary<string, string> {
    { "Aiden", "EndPortraits/Aiden" },
    {"Aborah", "EndPortraits/Aborah"},
    { "Jean", "EndPortraits/Jean" },
    { "Rene", "EndPortraits/Rene" },
    { "Architect", "EndPortraits/Architect" },
    { "Entropy", "EndPortraits/Entrophy"},
    { "Concierge", "EndPortraits/Concierge"},
    { "Estate", "EndPortraits/Estate"}
}; 
    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        if (!displayOnStart)
        {
            root.style.display = DisplayStyle.None;
        }
        display("Aiden");
        root.Q<UnityEngine.UIElements.Button>("Aiden").clicked += () => display("Aiden");
        root.Q<UnityEngine.UIElements.Button>("Aborah").clicked += () => display("Aborah");
        root.Q<UnityEngine.UIElements.Button>("Jean").clicked += () => display("Jean");
        root.Q<UnityEngine.UIElements.Button>("Rene").clicked += () => display("Rene");
        root.Q<UnityEngine.UIElements.Button>("Architect").clicked += () => display("Architect");
        root.Q<UnityEngine.UIElements.Button>("Entropy").clicked += () => display("Entropy");
        root.Q<UnityEngine.UIElements.Button>("Concierge").clicked += () => display("Concierge");
        root.Q<UnityEngine.UIElements.Button>("Estate").clicked += () => display("Estate");
    }

    private void display(string character) {
        root.Q<Label>("Name").text = titles[character];
        root.Q<Label>("Flavor").text = flavor[character];
        root.Q<Label>("Description").text = desc[character];
        print("HERE" + Resources.Load<Texture2D>(images[character]));
        root.Q<VisualElement>("Image").style.backgroundImage = Resources.Load<Texture2D>(images[character]);
    }

}

