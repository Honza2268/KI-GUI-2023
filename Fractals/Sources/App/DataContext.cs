using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using QuickStart;

namespace Fractals {
    public class DataContext : INotifyPropertyChanged {
        private int progressValue;
        private List<FractalGenerator> fractalGenerators = new();
        private FractalGenerator? selectedGenerator;
        private double fractalOriginX;
        private double fractalOriginY;

        public int ProgressValue {
            get => progressValue;
            set {
                if (value == progressValue) return;
                progressValue = value;
                OnPropertyChanged();
            }
        }

        public List<FractalGenerator> FractalGenerators {
            get => fractalGenerators;
            set {
                if (Equals(value, fractalGenerators)) return;
                fractalGenerators = value;
                OnPropertyChanged();
            }
        }
        
        public FractalGenerator? SelectedGenerator {
            get => selectedGenerator;
            set {
                if (value == selectedGenerator) return;
                selectedGenerator = value;
                OnPropertyChanged();
            }
        }

        public double FractalOriginX {
            get => fractalOriginX;
            set {
                if (value == fractalOriginX) return;
                fractalOriginX = value;
                OnPropertyChanged();
            }
        }
        
        public double FractalOriginY {
            get => fractalOriginY;
            set {
                if (value == fractalOriginY) return;
                fractalOriginY = value;
                OnPropertyChanged();
            }
        } 

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
