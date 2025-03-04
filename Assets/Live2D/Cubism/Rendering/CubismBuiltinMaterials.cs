/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using UnityEngine;

namespace Live2D.Cubism.Rendering
{
    /// <summary>
    ///     Default materials.
    /// </summary>
    public static class CubismBuiltinMaterials
    {
        /// <summary>
        ///     Default unlit material.
        /// </summary>
        public static Material Unlit => LoadUnlitMaterial("Unlit");

        /// <summary>
        ///     Default unlit, additively blending material.
        /// </summary>
        public static Material UnlitAdditive => LoadUnlitMaterial("UnlitAdditive");

        /// <summary>
        ///     Default unlit, multiply blending material.
        /// </summary>
        public static Material UnlitMultiply => LoadUnlitMaterial("UnlitMultiply");


        /// <summary>
        ///     Default unlit masked material.
        /// </summary>
        public static Material UnlitMasked => LoadUnlitMaterial("UnlitMasked");

        /// <summary>
        ///     Default unlit masked, additively blending material.
        /// </summary>
        public static Material UnlitAdditiveMasked => LoadUnlitMaterial("UnlitAdditiveMasked");

        /// <summary>
        ///     Default unlit masked, multiply blending material.
        /// </summary>
        public static Material UnlitMultiplyMasked => LoadUnlitMaterial("UnlitMultiplyMasked");


        /// <summary>
        ///     Default unlit masked inverted material.
        /// </summary>
        public static Material UnlitMaskedInverted => LoadUnlitMaterial("UnlitMaskedInverted");

        /// <summary>
        ///     Default unlit masked inverted, additively blending material.
        /// </summary>
        public static Material UnlitAdditiveMaskedInverted => LoadUnlitMaterial("UnlitAdditiveMaskedInverted");

        /// <summary>
        ///     Default unlit masked inverted, multiply blending material.
        /// </summary>
        public static Material UnlitMultiplyMaskedInverted => LoadUnlitMaterial("UnlitMultiplyMaskedInverted");


        /// <summary>
        ///     Default unlit material.
        /// </summary>
        public static Material UnlitCulling => LoadUnlitMaterial("UnlitCulling");

        /// <summary>
        ///     Default unlit, additively blending material.
        /// </summary>
        public static Material UnlitAdditiveCulling => LoadUnlitMaterial("UnlitAdditiveCulling");

        /// <summary>
        ///     Default unlit, multiply blending material.
        /// </summary>
        public static Material UnlitMultiplyCulling => LoadUnlitMaterial("UnlitMultiplyCulling");


        /// <summary>
        ///     Default unlit masked material.
        /// </summary>
        public static Material UnlitMaskedCulling => LoadUnlitMaterial("UnlitMaskedCulling");

        /// <summary>
        ///     Default unlit masked, additively blending material.
        /// </summary>
        public static Material UnlitAdditiveMaskedCulling => LoadUnlitMaterial("UnlitAdditiveMaskedCulling");

        /// <summary>
        ///     Default unlit masked, multiply blending material.
        /// </summary>
        public static Material UnlitMultiplyMaskedCulling => LoadUnlitMaterial("UnlitMultiplyMaskedCulling");


        /// <summary>
        ///     Default unlit masked inverted material.
        /// </summary>
        public static Material UnlitMaskedInvertedCulling => LoadUnlitMaterial("UnlitMaskedInvertedCulling");

        /// <summary>
        ///     Default unlit masked inverted, additively blending material.
        /// </summary>
        public static Material UnlitAdditiveMaskedInvertedCulling =>
            LoadUnlitMaterial("UnlitAdditiveMaskedInvertedCulling");

        /// <summary>
        ///     Default unlit masked inverted, multiply blending material.
        /// </summary>
        public static Material UnlitMultiplyMaskedInvertedCulling =>
            LoadUnlitMaterial("UnlitMultiplyMaskedInvertedCulling");


        /// <summary>
        ///     Default mask material.
        /// </summary>
        public static Material Mask => LoadMaskMaterial();

        /// <summary>
        ///     Default culled mask material.
        /// </summary>
        public static Material MaskCulling => LoadMaskCullingMaterial();


        #region Helper Methods

        /// <summary>
        ///     Resource directory of builtin <see cref="Material" />s.
        /// </summary>
        private const string ResourcesDirectory = "Live2D/Cubism/Materials";


        /// <summary>
        ///     Loads an unlit material.
        /// </summary>
        /// <param name="name">Material name.</param>
        /// <returns>The material.</returns>
        private static Material LoadUnlitMaterial(string name)
        {
            return Resources.Load<Material>(ResourcesDirectory + "/" + name);
        }

        /// <summary>
        ///     Loads an mask material.
        /// </summary>
        /// <returns>The material.</returns>
        private static Material LoadMaskMaterial()
        {
            return Resources.Load<Material>(ResourcesDirectory + "/Mask");
        }

        /// <summary>
        ///     Loads an mask culling material.
        /// </summary>
        /// <returns>The material.</returns>
        private static Material LoadMaskCullingMaterial()
        {
            return Resources.Load<Material>(ResourcesDirectory + "/MaskCulling");
        }

        #endregion
    }
}
