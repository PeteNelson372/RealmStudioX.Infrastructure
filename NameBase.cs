/**************************************************************************************************************************
* Copyright 2024, Peter R. Nelson
*
* This file is part of the RealmStudio application. The RealmStudio application is intended
* for creating fantasy maps for gaming and world building.
*
* RealmStudio is free software: you can redistribute it and/or modify it under the terms
* of the GNU General Public License as published by the Free Software Foundation,
* either version 3 of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
* without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
* See the GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License along with this program.
* The text of the GNU General Public License (GPL) is found in the LICENSE.txt file.
* If the LICENSE.txt file is not present or the text of the GNU GPL is not present in the LICENSE.txt file,
* see https://www.gnu.org/licenses/.
*
* For questions about the RealmStudio application or about licensing, please email
* support@brookmonte.com
*
***************************************************************************************************************************/
using System.ComponentModel;

namespace RealmStudioX
{
    public sealed class NameBase : INameGenerator, INotifyPropertyChanged
    {
        public Guid NameBaseGuid { get; set; } = Guid.NewGuid();
        public string NameBaseName { get; set; } = string.Empty;

        private bool _isNameBaseSelected = true;
        public bool IsNameBaseSelected
        {
            get => _isNameBaseSelected;

            set
            {
                if (_isNameBaseSelected != value)
                {
                    _isNameBaseSelected = value;

                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(
                            nameof(IsNameBaseSelected)));
                }
            }
        }

        public List<NameBaseLanguage> Languages { get; set; } = [];

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
