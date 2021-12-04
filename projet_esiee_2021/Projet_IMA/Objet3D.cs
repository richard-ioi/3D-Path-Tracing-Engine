﻿using System;
namespace Projet_IMA
{
    abstract class Objet3D
    {
        protected V3 m_CentreObjet { get; set; } 
        private Texture m_Texture { get; set; }
        private Texture m_BumpTexture { get; set; }
        private float m_CoefficientDiffus { get; set; }
        private float m_CoefficientSpeculaire { get; set; }
        private float m_PuissanceSpeculaire { get; set; }
        private float m_CoefficientBumpMap { get; set; }
        protected float m_Pas { get; set; }

        #region Constructeur
        /// <summary>
        /// Constructeur d'un objet 3D
        /// </summary>
        /// <param name="centre">Centre de l'Objet3D</param>
        /// <param name="lumiere">Lumière appliquée sur l'Objet3D</param>
        /// <param name="texture">Texture appliquée sur l'Objet3D</param>
        /// <param name="bump_texture">Texture de bump appliquée sur l'Objet3D</param>
        /// <param name="coefficient_diffus">Coefficient de diffus de la sphère, plus le coefficient est faible, plus le diffus sera "fondu"</param>
        /// <param name="coefficient_speculaire">Coefficient spéculaire, plus le coefficient est faible, plus le spéculaire sera "fondu"</param>
        /// <param name="puissance_speculaire">Puissance spéculaire, plus la puissance est élevée, moins le spéculaire sera grand</param>
        /// <param name="coefficient_bumpmap">Coefficient de Bump Mapping, plus il sera élevé, plus l'effet 3D sera élevé.</param>
        /// <param name="pas">Ecart entre le placement des pixels de l'objet. Plus l'écart est grand, moins de pixels seront dessinés.</param>
        public Objet3D(V3 centre, Texture texture, Texture bump_texture, float coefficient_diffus, float coefficient_speculaire, float puissance_speculaire, float coefficient_bumpmap, float pas)
        {
            m_CentreObjet = centre;
            m_CoefficientDiffus = coefficient_diffus;
            m_CoefficientSpeculaire = coefficient_speculaire;
            m_PuissanceSpeculaire = puissance_speculaire;
            m_Texture = texture;
            m_BumpTexture = bump_texture;
            m_CoefficientBumpMap = coefficient_bumpmap;
            m_Pas = pas;
        }
        #endregion

        #region Méthodes

        /// <summary>
        /// Calcule les coordonnées du Pixel 3D de l'objet grâce aux positions u et v sur la texture 2D.
        /// </summary>
        /// <param name="u">Coordonnées en abscisses de la texture l'objet</param>
        /// <param name="v">Coordonnées en ordonnées de la texture l'objet</param>
        /// <returns></returns>
        protected abstract V3 getCoords(float u, float v);
        protected abstract void getDerivedCoords(float u, float v, out V3 dMdu, out V3 dMdv);
        /// <summary>
        /// Calcule la normale du pixel passé en paramètre
        /// </summary>
        /// <param name="PixelPosition">Position du pixel dont on veut obtenir la normale</param>
        /// <returns>Normale du pixel passé en paramètre</returns>
        protected abstract V3 getNormal(V3 PixelPosition);

        /// <summary>
        /// Classe abstraite définissant comment dessiner l'objet héritant de cette classe
        /// </summary>
        /// <param name="pas">Écart entre chaque point tracé à l'écran</param>
        public abstract void Draw();
        public abstract bool IntersectionRayon(V3 origineRayon, V3 directionRayon, out float t, out V3 PixelPosition, out float u, out float v);

        /// <summary>
        /// Renvoie la couleur ambiante du pixel correspondant aux coordonnées de la texture de l'objet.
        /// </summary>
        /// <param name="u">Coordonnées en abscisses de la texture l'obje</param>
        /// <param name="v">Coordonnées en ordonnées de la texture l'obje</param>
        /// <returns>Couleur ambiante du pixel passé en paramètre</returns>
        private Couleur getCouleurAmbiante(Lumiere lumiere, float u, float v)
        {
            return m_Texture.LireCouleur(u, v) * lumiere.m_Couleur;
        }

        /// <summary>
        /// Renvoie la couleur ambiante attenuée du pixel correspondant aux coordonnées de la texture de l'objet.
        /// </summary>
        /// <param name="u">Coordonnées en abscisses de la texture l'obje</param>
        /// <param name="v">Coordonnées en ordonnées de la texture l'obje</param>
        /// <returns>Couleur ambiante attenuée du pixel passé en paramètre</returns>
        public Couleur getLowCouleurAmbiante(Lumiere lumiere, float u, float v)
        {
            return getCouleurAmbiante(lumiere, u,v) * .0008f;
        }

        /// <summary>
        /// Calcule la couleur diffuse du pixel passé en paramère
        /// </summary>
        /// <param name="normalizedPixelNormal">Vecteur décrivant la normale au point x,y</param>
        /// <param name="x_ecran">Positionnement en X sur l'écran du point interrogé</param>
        /// <param name="y_ecran">Positionnement en Y sur l'écran du point interrogé</param>
        /// <returns>Couleur diffuse du pixel passé en paramètre</returns>
        private Couleur getCouleurDiffuse(Lumiere lumiere, V3 pixelNormal, float u, float v)
        {
            float cosAlpha = pixelNormal * lumiere.m_NormalizedDirection;
            if (cosAlpha > 0)
            {
                return getCouleurAmbiante(lumiere,u , v)  * (cosAlpha) * m_CoefficientDiffus;
            }
            else
            {
                return Couleur.m_Void;
            }
        }

        /// <summary>
        /// Calcule la couleur spéculaire du pixel passé en paramère
        /// </summary>
        /// <param name="PixelPosition">Position du pixel dont on veut trouver la couleur spéculaire</param>
        /// <param name="N">Normale associée au pixel passé en paramètre</param>
        /// <param name="x_ecran">Position x de l'écran du pixel passé en paramètre</param>
        /// <param name="y_ecran">Position y de l'écran du pixel passé en paramètre</param>
        /// <returns>Couleur spéculaire du pixel passé en paramètre</returns>
        private Couleur getCouleurSpeculaire(Lumiere lumiere, V3 PixelPosition, V3 N, float u, float v)
        {
            V3 L = lumiere.m_Direction;
            V3 R = 2*N*(N*L)-L;
            V3 D = (BitmapEcran.s_CameraPosition - PixelPosition);
            R.Normalize();
            D.Normalize();
            float RD = R * D;
            if ((RD) > 0)
            {
                return lumiere.m_Couleur * getCouleurAmbiante(lumiere, u, v) * m_CoefficientSpeculaire * (float)Math.Pow(RD, m_PuissanceSpeculaire);
            }
            else
            {
                return Couleur.m_Void;
            }
        }

        /// <summary>
        /// Calcule la couleur totale du pixel passé en paramère
        /// </summary>
        /// <param name="PixelPosition">Position du pixel dont on veut trouver la couleur spéculaire</param>
        /// <param name="u">Position du vecteur u qui pointe sur les coordonnées en abscisses de la texture l'objet</param>
        /// <param name="v">Position du vecteur v qui pointe sur le pixel de l'objet</param>
        /// <returns>Couleur totale du pixel passé en paramètre</returns>
        public Couleur getCouleur(V3 PixelPosition, float u, float v)
        {
            Couleur finalColor = Couleur.m_Void;
            foreach (Lumiere lumiere in BitmapEcran.s_Lumieres) {
                V3 N = getBumpedNormal(PixelPosition, u, v);
                Couleur Ambiant = getLowCouleurAmbiante(lumiere, u, v);
                Couleur Diffus = getCouleurDiffuse(lumiere, N, u, v);
                if (isInShadow(lumiere.m_Direction, PixelPosition))
                {
                    finalColor += Ambiant;
                }
                else
                {
                    if (Diffus != Couleur.m_Void)
                    {
                        Couleur Speculaire = getCouleurSpeculaire(lumiere, PixelPosition, N, u, v);
                        finalColor += Ambiant + Diffus + Speculaire;
                    }
                    else
                    {
                        finalColor += Ambiant + Diffus;
                    }
                }
            }
            return finalColor;
        }

        /// <summary>
        /// Calcule la normale bumpée du pixel actuel grâce à la texture de bumping de l'objet
        /// </summary>
        /// <param name="PixelPosition"></param>
        /// <param name="u">Position du vecteur u qui pointe sur les coordonnées en abscisses de la texture l'objet</param>
        /// <param name="v">Position du vecteur v qui pointe sur les coordonnées en ordonnées de la texture de l'objet</param>
        /// <returns>Normale bumpée du pixel actuel</returns>
        private V3 getBumpedNormal(V3 PixelPosition, float u, float v)
        {
            V3 N = getNormal(PixelPosition);

            float K = m_CoefficientBumpMap;
            getDerivedCoords(u, v, out V3 dMdu, out V3 dMdv);
            this.m_BumpTexture.Bump(u, v, out float dhdu, out float dhdv);

            return N + K * ((dMdu ^ (N * dhdv)) + ((N * dhdu) ^ dMdv));
        }

        /// <summary>
        /// Permet de déterminer si le pixel de l'objet est obstrué par un autre objet qui lui cache la lumière passée en paramètre
        /// </summary>
        /// <param name="_lumiere">Direction de la lumière dont on veut tester l'obstruction</param>
        /// <param name="_PixelPosition">Position du pixel dont on veut tester l'obstruction</param>
        /// <returns>Vrai si le pixel est obstrué, faux sinon.</returns>
        private bool isInShadow(V3 _lumiereDirection, V3 _PixelPosition)
        {
            foreach (Objet3D autres_objets in BitmapEcran.s_Objets)
            {
                if (autres_objets != this)
                {
                    if (autres_objets.IntersectionRayon(_PixelPosition, _lumiereDirection, out _, out V3 PixelPosition2, out _, out _))
                    {
                        if (BitmapEcran.s_CameraPosition - PixelPosition2 < BitmapEcran.s_CameraPosition - _PixelPosition)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
