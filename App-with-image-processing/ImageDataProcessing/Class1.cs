﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
namespace ImageDataProcessing
{
    public class FrameHeaderStrategyContext
    {
        /* El contexto define la interfaz de interes para el usuario.
         * Mantiene una referencia a la cual acceder al momento de requerir alguna estrategia.
         * El contexto no sabe que clase concreta se implementara por
         * estrategia. Debe trabajar para todas las estrategias via 
         * IFrameHeaderFormatStrategy interface
        */
        
        Dictionary<string, IFrameHeaderFormatStrategy> strategyContext = 
            new Dictionary<string, IFrameHeaderFormatStrategy>();

        public FrameHeaderStrategyContext()
        {
            // Agrega a un diccionario las estrategias que se implementan
            // mediante la interfaz IFrameHeaderFormatStrategy
            strategyContext.Add(nameof(BMPHeaderFormat),
                new BMPHeaderFormat());
            strategyContext.Add(nameof(OtherHeaderFormat),
                    new OtherHeaderFormat());
        }

        public Dictionary<string, string> ApplyStrategy(IFrameHeaderFormatStrategy strategy, BinaryReader Data, string imagen)
        {
            /*Actualmente, ApplyStrategy tiene una implementación simple.
             * Es necesario que primero se ejecute GetStrategy para que se seleccione
             * la estrategia a utilizar al determinar dependiendo del formato de la imagen
            */
            return strategy.FrameHeaderReader(Data, imagen);
        }

        public IFrameHeaderFormatStrategy GetStrategy(BinaryReader Data)
        {
            /*
            En ausencia de este metodo se tendria que determinar el tipo de 
            archivo en cualquier lugar fuera del dll.
            Context actua como un punto unico de contacto donde el cliente puede
            que ejecutar las distintas estrategias
            */
            string bm = String.Concat(Convert.ToChar(Data.ReadByte()), Convert.ToChar(Data.ReadByte()));
            if (bm == "BM")
            {
                return strategyContext[nameof(BMPHeaderFormat)];
            }
            else
            {
                // Cualquier otro formato lo ejecutara la otra estrategia de extracción de datos
                return strategyContext[nameof(OtherHeaderFormat)];
            }
            // Si se quieren agregar especificar otras estrategias
            // especificas para otros formatos  solo se necesita saber su numero 
            // magico o el identificador de la cabecera y la logica se implementa mediante la interface
        }
    }

    public interface IFrameHeaderFormatStrategy
    {
        /* Esta interfaz recibe como input la data de una imagen
         * y dependiendo del formato se extrae la cabecera mediante 
         * distintas  estrategias para retornar un diccionario 
         * cuyo key = nombre del metadato y el value = valor extraido
         */
        Dictionary<string, string> FrameHeaderReader(BinaryReader Data, string imagen);
    }

    
    public class BMPHeaderFormat : IFrameHeaderFormatStrategy
    {
        /* Esta clase implementa la interface IFrameHeaderFormatStrategy,
         * con el fin de encapsular la lógica de crear el header para imagenes 
         * BMP utilizando el patron de diseño strategy
        */
       
        // Implementación de IFrameHeaderFormatStrategy<T> interface
        public Dictionary<string, string> FrameHeaderReader(BinaryReader Data, string imagen)
        {
            Dictionary<string, string> headerAssembly = new Dictionary<string, string>();
            Data.BaseStream.Seek(0, SeekOrigin.Begin);
            // Los siguientes parametros se hallan recorriendo la cabecera de la imagen mediante la clase BinaryReader
            string fichero = String.Concat(Convert.ToChar(Data.ReadByte()), Convert.ToChar(Data.ReadByte()));
            headerAssembly.Add("Tipo de fichero", fichero);
            headerAssembly.Add("Tamaño del archivo", Convert.ToString(Data.ReadInt32())); // Su valor es 3 veces el tamaño de la imagen + cabecera 
                                                                                          // debido a que se toman en cuenta los 3 canales de color
            Data.BaseStream.Seek(10, SeekOrigin.Begin);
            headerAssembly.Add("Inicio de los datos de la imagen", Convert.ToString(Data.ReadInt32()));
            headerAssembly.Add("Tamaño de la cabecera de bitmap", Convert.ToString(Data.ReadInt32()));
            headerAssembly.Add("Ancho", Convert.ToString(Data.ReadInt32()));
            headerAssembly.Add("Alto", Convert.ToString(Data.ReadInt32()));

            Data.BaseStream.Seek(30, SeekOrigin.Begin);
            int compresion = Data.ReadInt32();
            switch (compresion)
            {
                case 0:
                    headerAssembly.Add("compresión", "Sin compresión"); 
                    break;
                case 1:
                    headerAssembly.Add("compresión", "Compresión RLE 8 bits"); 
                    break;
                case 2:
                    headerAssembly.Add("compresión", "Compresión RLE 4 bits");  
                    break;
            }
            headerAssembly.Add("Tamaño de la imagen", Convert.ToString(Data.ReadInt32() / 3)); // Se divide entre 3 debido a que cada pixel
                                                                                               // contiene 3 bites
            headerAssembly.Add("Resolución horizontal", Convert.ToString(Data.ReadInt32()));
            headerAssembly.Add("Resolución vertical", Convert.ToString(Data.ReadInt32()));
            // Los siguientes parametros se hallan mediante lo que entrega la clase Bitmap
            Bitmap imgBit = new Bitmap(imagen);
            headerAssembly.Add("Formato de Pixel (Bitmap)", imgBit.PixelFormat.ToString());
            headerAssembly.Add("Ancho (Bitmap)", imgBit.Width.ToString());
            headerAssembly.Add("Alto (Bitmap)", imgBit.Height.ToString());
            headerAssembly.Add("Tamaño de la imagen (Bitmap)", (imgBit.Width * imgBit.Height).ToString());
            headerAssembly.Add("Resolución horizontal (Bitmap)", imgBit.HorizontalResolution.ToString());
            headerAssembly.Add("Resolución vertical (Bitmap)", imgBit.VerticalResolution.ToString());
            return headerAssembly;
        }
    }

    public class OtherHeaderFormat : IFrameHeaderFormatStrategy
    {
        /* Esta clase implementa la interface IFrameHeaderFormatStrategy,
         * con el fin de encapsular la lógica de crear el header para imagenes 
         * distintas a .BMP utilizando el patron de diseño strategy
        */
        // Implementación de IFrameHeaderFormatStrategy<T> interface
        public Dictionary<string, string> FrameHeaderReader(BinaryReader Data, string imagen)
        {
            Dictionary<string, string> headerAssembly = new Dictionary<string, string>();
            
            Bitmap imgBit = new Bitmap(imagen);
            headerAssembly.Add("Formato de Pixel", imgBit.PixelFormat.ToString());
            headerAssembly.Add("Ancho", imgBit.Width.ToString());
            headerAssembly.Add("Alto", imgBit.Height.ToString());
            headerAssembly.Add("Tamaño de la imagen", (imgBit.Width * imgBit.Height).ToString());
            headerAssembly.Add("Resolución horizontal", imgBit.HorizontalResolution.ToString());
            headerAssembly.Add("Resolución vertical", imgBit.VerticalResolution.ToString());
            return headerAssembly;
        }
    }
}
