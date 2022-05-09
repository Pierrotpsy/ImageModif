# Rapport PSI

### Instructions de lancement
Vérifier que les trois solutions du projet sont bien chargées et exécuter la solution `GUI_PSI`.

### Introduction
Ce projet suit le cahier des charges réalisé pour le module `Problème scientifique informatique`. Il contient les fonctionnalités suivantes : 
1. Lire et écrire un fichier BMP
2. Convertir une image en nuances de gris
3. Convertir une image en noir et blanc
4. Agrandir ou rétrécir une image
5. Appliquer l'effet miroir à une image
6. Effectuer la rotation d'une image à n'importe quel degré
7. Générer une fractale de Mandelbrot
8. Utilisation d'une matrice de convolution pour :
   -  Détection de contours (3 niveaux)
   -  Renforcement des bords
   -  Flou
   - Augmenter le contraste
   - Repoussage
9. Générer un histogramme se rapportant à une image

### Structure du code
* `PSI` -> Solution principale du projet
    - `MyImage` : classe principale , elle contient toutes les méthodes pour lire, modifier, et écrire une image.
    - `QRcode`: classe pour générer un QR code, elle contient toutes les méthodes pour réaliser cet objectif.
    - `RGB` : classe auxiliaire utilisée pour stocker les couleurs des pixels lors de la lecture et de l'écriture d'images.
    - `Complex` : classe auxiliaire utilisée pour l'algorithme générant la fractale de mandelbrot.
    - `GenericGF` : classe fournie pour faire fonctionner l'algorithme de Reed-Solomon
    - `GenericGFPoly`: classe fournie pour faire fonctionner l'algorithme de Reed-Solomon
    - `ReedSolomonAlgorithm`: classe fournie pour faire fonctionner l'algorithme de Reed-Solomon
    - `ReedSolomonEncoder`: classe fournie pour faire fonctionner l'algorithme de Reed-Solomon
    - `ReedSolomonDecoder`: classe fournie pour faire fonctionner l'algorithme de Reed-Solomon
    - `Histogram` : classe pour générer un histogramme décrivant une image à partir de cette image.
    - `Program` : programme pour exécuter 
* `GUI_PSI` -> Solution pour l'affichage WPF : 
* `PSI_Test`-> Solution pour les tests unitaires 
    - `MyImageTest` : teste les fonctions basiques de la classe

### Innovation
Pour l'innovation, nous avons décidé d'approfondir le QR code et de le faire fonctionner pour toutes les versions (1 à 40), tous les niveaux de correction (L, M, Q et H) en prenant en compte tous les masques (0 à 7). Les niveaux de correction qui fonctionnent le mieux sont L et M, mais les deux autres fonctionnent également.
Pour lancer ces QR code en forçant le niveau de correction, il faut lancer le code par `PSI` dans `Program` et non par `GUI_PSI` car nous n'avons pas eu le temps de le rajouter au WPF. 


### Problèmes rencontrés
Les problèmes que nous avons rencontré sont les suivants : 
- arrondir le nombre de pixels en hauteur et largeur au multiple de 4 le plus proche.
- pour la matrice de convolution, l'application des différents filtres a causé un certain nombre de problèmes initialement.
- pour le QR code, nous avons eu quelques difficultés à le faire fonctionner correctement (lecture par un téléphone)

### Auto-critique
Nous avions mal lu l'énoncé pour le QR code, et nous avons immédiatement cherché à coder les 8 types de masques, qui prennent donc en compte la pénalité par masque pour choisir le plus optimisé.

De plus, l'interface WPF reste très basique. Nous aurions aimé en faire une plus développée mais nous n'avons pas eu le temps de la terminer et avons donc rendu celle-ci qui est fonctionnelle.

### Etude de compression d'images

Il existe deux types de compressions, la compressions avec ou sans perte.

###### Les compressions avec pertes :

Dans cette méthode on réalise un approximation des valeurs. 
Par exmple la méthode de la fractale utilise le fait que les parties d'une image peuvent être similaires et ainsi il est possible de réduire l'image à des éléments principaux que le l'ont transforme afin d'obtenir l'image en elle même. Il suffit alors de coder ces éléments puis leurs transformations. 

###### Les compressions sans pertes :

Prenons en exemple le codage de Huffman, le principe est de lire le fichier et de comptrer le nombre d'occurences afin de réaliser une table de fréqeunces. Ainsi on code les appartitions rares avec une longueur binaire élevée et celles plus fréquentes avec une longueur faible. On construit une structure en arbre. Cette méthode présente des inconvénients de complexités en effets il y a plusieurs étapes à réaliser et ces étapes sont trés gourmandes en CPU.
