import sys
import os
import base64
from io import BytesIO
from PIL import Image
import torch

def log(message):
    print(message)
    sys.stdout.flush()

log("Starting script...")

# Ensure the correct clip module is being imported
try:
    import clip
    log("Clip module imported successfully.")
except ImportError as e:
    log(f"Failed to import clip: {e}")
    sys.exit(1)

log("Clip module attributes: " + str(dir(clip)))

def get_image_features(image_data):
    log("Entered get_image_features function.")
    
    log(f"Current working directory: {os.getcwd()}")
    log(f"Python executable: {sys.executable}")
    log(f"Python version: {sys.version}")
    log(f"Clip module location: {clip.__file__}")
    
    if torch.cuda.is_available():
        device = "cuda"
        log(f"Using GPU: {torch.cuda.get_device_name(torch.cuda.current_device())}")
    else:
        device = "cpu"
        log("Using CPU")

    # Attempt to load the model and preprocess function
    try:
        model, preprocess = clip.load("ViT-B/32", device=device)
        log("Model and preprocess function loaded successfully.")
    except AttributeError as e:
        log(f"Error loading model: {e}")
        sys.exit(1)
    
    log("Decoding image data...")
    try:
        image = Image.open(BytesIO(base64.b64decode(image_data)))
        image = preprocess(image).unsqueeze(0).to(device)
        log("Image data decoded and preprocessed.")
    except Exception as e:
        log(f"Error decoding image data: {e}")
        sys.exit(1)

    log("Extracting features...")
    try:
        with torch.no_grad():
            image_features = model.encode_image(image)
        log("Image features extracted.")
    except Exception as e:
        log(f"Error extracting image features: {e}")
        sys.exit(1)

    return image_features, model

def rank_captions(image_features, model):
    log("Entered rank_captions function.")
    
    captions = [
    "a fireball spell", "a healing spell", "a lightning spell", "a shield spell", "a damage spell", 
    "a storm of lightning spell", "an ice projectile spell", "a water spell", "a wind spell", "an earth spell", 
    "a poison spell", "a firestorm spell", "a blizzard spell", "a thunderbolt spell", "a stone skin spell", 
    "a barrier spell", "a magic missile spell", "a summon spell", "a curse spell", "a blessing spell", 
    "a transformation spell", "a petrification spell", "an explosion spell", "a light spell", "a dark spell", 
    "a psychic spell", "a mind control spell", "a gravity spell", "a time manipulation spell", "a sound wave spell", 
    "an invisibility spell", "a speed spell", "a slow spell", "a meteor shower spell", "a sunbeam spell", 
    "a moonbeam spell", "a necromancy spell", "a rejuvenation spell", "a barrier of light spell", "a shadow cloak spell", 
    "a venomous strike spell", "a gale force spell", "a tsunami spell", "an earthquake spell", "a vine grasp spell", 
    "a frostbite spell", "a magma burst spell", "a spectral hand spell", "a shockwave spell", "a charm spell", 
    "an illusion spell", "a shape-shifting spell", "a magic circle spell", "a summoning circle spell", "an arcane blast spell", 
    "a fire pillar spell", "a freezing wind spell", "a storm cloud spell", "a radiant beam spell", "a spectral blade spell", 
    "an electric surge spell", "a lunar shield spell", "a solar flare spell", "a blazing inferno spell", "a hurricane spell", 
    "a tidal wave spell", "a sandstorm spell", "a healing rain spell", "a rejuvenating light spell", "a burning hands spell", 
    "a thunder strike spell", "a blinding light spell", "a dark shadow spell", "a venom cloud spell", "a quicksand spell", 
    "a rock slide spell", "a mind blast spell", "a spirit call spell", "a blood magic spell", "a celestial spell", 
    "a void spell", "a dream spell", "a nightmare spell", "a force field spell", "a magical armor spell", 
    "a phantom spell", "a specter spell", "a guardian spell", "a torment spell", "a salvation spell", 
    "a blessing of strength spell", "a protection spell", "an enchantment spell", "a dispel magic spell", "a counter spell", 
    "a mana drain spell", "a mana boost spell", "a rage spell", "a tranquility spell", "a freezing touch spell", 
    "a scorching ray spell", "a mind shield spell", "a resurrection spell", "a soul capture spell", "a binding spell", 
    "a weakening spell", "an empowerment spell", "a healing touch spell", "a barrier of flame spell", "a wall of ice spell", 
    "a whirlpool spell", "a tornado spell", "a lightning strike spell", "a pyroclasm spell", "a meteor impact spell", 
    "a flash of light spell", "a shadow step spell", "a venom dart spell", "a starlight spell", "a moonlight spell", 
    "a burning rage spell", "a freezing cold spell", "a glowing aura spell", "a spectral light spell", "a dark flame spell", 
    "a poisonous mist spell", "a storm of arrows spell", "a blizzard blast spell", "a fire wall spell", "a storm surge spell", 
    "a tidal surge spell", "a hurricane force spell", "a volcanic eruption spell", "a spirit shield spell", "a soul strike spell", 
    "a mystical bolt spell", "an arcane shield spell", "a healing wind spell", "a toxic cloud spell", "a firestorm blast spell", 
    "a frost storm spell", "a dark void spell", "a blinding flash spell", "a telekinetic force spell", "an electric arc spell", 
    "a necrotic touch spell", "a spectral force spell", "a rejuvenation wave spell", "a stone barrier spell", "a quicksilver spell", 
    "a venomous breath spell", "a psychic scream spell", "a solar beam spell", "a lunar light spell", "a storm call spell", 
    "a fire nova spell", "a frost nova spell", "a shadow veil spell", "a blood ritual spell", "a celestial light spell", 
    "a dreamweaver spell", "a nightmare weave spell", "a guardian shield spell", "a tormentor spell", "a savior spell", 
    "a strength boon spell", "a protection aura spell", "an enchantment aura spell", "a dispelling ward spell", "a counter ward spell", 
    "a mana siphon spell", "a mana surge spell", "a berserk spell", "a calm spell", "a frost touch spell", 
    "a flame touch spell", "a mind barrier spell", "a rebirth spell", "a soul bind spell", "a chains of magic spell", 
    "a drain life spell", "an empower spell", "a divine touch spell", "a wall of flames spell", "an ice wall spell", 
    "a whirlpool vortex spell", "a storm vortex spell", "a lightning storm spell", "a volcanic blast spell", "a meteor storm spell", 
    "a flare spell", "a step through shadows spell", "a venom spray spell", "a starburst spell", "a moonbeam burst spell", 
    "a flaming rage spell", "a chilling cold spell", "a radiant aura spell", "a dark aura spell", "a poison mist spell", 
    "a storm of bolts spell", "a frost wave spell", "a wall of fire spell", "a storm blast spell", "a tidal wave blast spell", 
    "a hurricane blast spell", "a volcanic eruption blast spell", "a spirit guard spell", "a soul guard spell", 
    "a mystic bolt spell", "an arcane ward spell", "a healing breeze spell", "a toxic haze spell", "a firestorm wave spell", 
    "a frost storm wave spell", "a dark vortex spell", "a blinding light spell", "a telekinetic blast spell", 
    "an electric surge spell", "a necrotic blast spell", "a spectral surge spell", "a rejuvenation aura spell", 
    "a stone wall spell", "a quicksilver burst spell", "a venom cloud burst spell", "a psychic blast spell", 
    "a solar flare burst spell", "a lunar light burst spell", "a storm caller spell", "a fire nova burst spell", 
    "a frost nova burst spell", "a shadow shroud spell", "a blood magic burst spell", "a celestial burst spell", 
    "a dream burst spell", "a nightmare burst spell", "a guardian burst spell", "a torment burst spell", "a salvation burst spell", 
    "a strength burst spell", "a protection burst spell", "an enchantment burst spell", "a dispelling burst spell", 
    "a counter spell burst", "a mana drain burst spell", "a mana surge burst spell", "a berserk burst spell", "a calm burst spell", 
    "a frost touch burst spell", "a flame touch burst spell", "a mind barrier burst spell", "a rebirth burst spell", 
    "a soul bind burst spell", "a chains of magic burst spell", "a drain life burst spell", "an empower burst spell", 
    "a divine touch burst spell", "a wall of flames burst spell", "an ice wall burst spell", "a whirlpool vortex burst spell", 
    "a storm vortex burst spell", "a lightning storm burst spell", "a volcanic blast burst spell", "a meteor storm burst spell", 
    "a flare burst spell", "a step through shadows burst spell", "a venom spray burst spell", "a starburst burst spell", 
    "a moonbeam burst burst spell", "a flaming rage burst spell", "a chilling cold burst spell", "a radiant aura burst spell", 
    "a dark aura burst spell", "a poison mist burst spell", "a storm of bolts burst spell", "a frost wave burst spell", 
    "a wall of fire burst spell", "a storm blast burst spell", "a tidal wave blast burst spell", "a hurricane blast burst spell", 
    "a volcanic eruption burst burst spell", "a spirit guard burst spell", "a soul guard burst spell", "a mystic bolt burst spell", 
    "an arcane ward burst spell", "a healing breeze burst spell", "a toxic haze burst spell", "a firestorm wave burst spell", 
    "a frost storm wave burst spell", "a dark vortex burst spell", "a blinding light burst spell", "a telekinetic blast burst spell", 
    "an electric surge burst spell", "a necrotic blast burst spell", "a spectral surge burst spell", "a rejuvenation aura burst spell", 
    "a stone wall burst spell", "a quicksilver burst spell", "a venom cloud burst spell", "a psychic blast burst spell", 
    "a solar flare burst spell", "a lunar light burst spell",
    "pointy and sharp", "round and bubbly", "glowing and radiant", "dark and shadowy", "smooth and sleek", 
    "rough and jagged", "sparkling and glittery", "smoky and misty", "fiery and blazing", "icy and frosty", 
    "windy and swirling", "earthy and solid", "watery and fluid", "crackling with electricity", "pulsating with energy", 
    "spiky and thorny", "glowing with runes", "enveloped in flames", "covered in frost", "shimmering with light", 
    "surrounded by shadows", "wrapped in vines", "radiating heat", "emitting cold", "glowing with auras", 
    "encircled by winds", "surging with power", "radiating with magic", "whirling in a vortex", "blazing with intensity", 
    "freezing with chill", "crackling with sparks", "flowing like water", "solid as rock", "whispering with spirits", 
    "burning with passion", "chilling with fear", "blinding with light", "obscured by darkness", "entwined with vines", 
    "glowing with mystical light", "emitting a ghostly glow", "bursting with energy", "calm and soothing", "wild and chaotic", 
    "steady and stable", "unpredictable and erratic", "ancient and wise", "fresh and new", "silent and still", 
    "booming and loud", "ethereal and otherworldly", "material and tangible", "arcane and mysterious", "primal and raw", 
    "refined and polished", "mighty and powerful", "gentle and soft", "intense and fierce", "delicate and fragile", 
    "vibrant and colorful", "dull and muted", "intricate and detailed", "simple and plain", "glowing with life", 
    "draining with decay", "bright and shining", "dim and fading", "energized and lively", "lethargic and slow", 
    "focused and direct", "scattered and random", "structured and orderly", "disorganized and chaotic", 
    "harmonious and balanced", "clashing and discordant",
    "red colored", "blue colored", "green colored", "yellow colored", "purple colored", "magenta colored", 
    "white colored", "black colored", "orange colored", "pink colored", "cyan colored", "brown colored", 
    "gray colored", "silver colored", "gold colored", "beige colored", "lime green colored", "maroon colored", 
    "navy blue colored", "turquoise colored", "teal colored", "violet colored", "amber colored", "bronze colored", 
    "crimson colored", "jade green colored", "lavender colored", "mint green colored", "olive green colored", 
    "peach colored", "plum colored", "salmon colored", "scarlet colored", "saffron colored", "indigo colored", 
    "chocolate brown colored", "charcoal gray colored", "midnight blue colored", "ivory colored", "rose colored", 
    "rust colored", "emerald green colored", "coral colored", "auburn colored", "azure colored", "burgundy colored", 
    "champagne colored", "chartreuse colored", "copper colored", "fuchsia colored", "goldenrod colored", 
    "khaki colored", "lavender blush colored", "lemon yellow colored", "lime colored", "mauve colored", 
    "pearl white colored", "periwinkle colored", "ruby red colored", "sage green colored", "sea green colored", 
    "slate gray colored", "tan colored", "tangerine colored", "taupe colored", "wheat colored"

    ]
    
    log("Tokenizing captions...")
    try:
        text = clip.tokenize(captions).to(image_features.device)
        log("Captions tokenized.")
    except Exception as e:
        log(f"Error tokenizing captions: {e}")
        sys.exit(1)
    
    log("Encoding captions...")
    try:
        with torch.no_grad():
            text_features = model.encode_text(text)
        log("Captions encoded.")
    except Exception as e:
        log(f"Error encoding captions: {e}")
        sys.exit(1)
    
    try:
        image_features = image_features / image_features.norm(dim=-1, keepdim=True)
        text_features = text_features / text_features.norm(dim=-1, keepdim=True)

        log("Calculating similarity scores...")
        similarities = (100.0 * image_features @ text_features.T).softmax(dim=-1)
        values, indices = similarities[0].topk(5)

        top_captions = [captions[i] for i in indices]
        log(f"Top captions determined: {top_captions}")
    except Exception as e:
        log(f"Error calculating similarity scores: {e}")
        sys.exit(1)

    return top_captions

def main():
    log("Entered main function.")
    
    if len(sys.argv) < 2:
        log("Usage: python ClipProcessor.py <image_data_file>")
        return

    image_data_file = sys.argv[1]

    log(f"Reading image data from {image_data_file}...")
    try:
        with open(image_data_file, "r") as file:
            image_data = file.read().strip()
        log("Image data read successfully.")
    except Exception as e:
        log(f"Error reading image data: {e}")
        sys.exit(1)

    image_features, model = get_image_features(image_data)
    top_captions = rank_captions(image_features, model)

    log(f"Image interpretation completed. Top captions: {top_captions}")

if __name__ == "__main__":
    main()