using Softhouse.SomeSpecificSerializer.Models;

namespace Softhouse.SomeSpecificSerializer;
public class FlatfileDeserializer
{
    public char _delimiter { get; set; }
    public const int EXPECTED_COLUMNS_PERSON = 3;
    public const int EXPECTED_COLUMNS_FAMILYMEMBER = 3;
    public const int EXPECTED_COLUMNS_TELEPHONE = 3;
    public const int EXPECTED_COLUMNS_ADDRESS = 4;
    public List<string> _errorLogs { get; set; } 
    public List<Person> _persons { get; set; }
    public Person _currentPerson { get; set; }
    public FamilyMember _currentFamilyMember { get; set; }
    public Human _currentHuman; // this will be assigned next incoming T or A
    public FlatfileDeserializer(char delimiter, List<string> errorLogs)
    {
        _delimiter = delimiter;
        _errorLogs = errorLogs;
        _persons = new List<Person>();
    }

    public Person CreatePerson()
    {
        _currentPerson = new Person() {
            FamilyMembers = new List<FamilyMember>()
        }; 
        
        _persons.Add(_currentPerson);

        return _currentPerson;
    }

    public FamilyMember AppendNewFamilyMemberToPerson()
    {
        _currentFamilyMember = new FamilyMember();
        _currentPerson.FamilyMembers.Add(_currentFamilyMember);

        return _currentFamilyMember;
    }

    public List<Person> DeserializeFlatfile(string filePath)
    {
        var flatFileRowType = string.Empty;
        var colIndexForType = 0;
        var rowIndex = 1;
        foreach (var entity in System.IO.File.ReadLines(filePath))
        {
            var entityValues = entity.Split(_delimiter); 
            var skipRow = false;
            for (int columnIndex = 0; (!skipRow && columnIndex < entityValues.Length); columnIndex++)
            {
                flatFileRowType = entityValues[colIndexForType];
                var columnCount = entityValues.Length;
                var isFirstColumn = columnIndex == 0;
                switch (flatFileRowType)
                {
                    case FlatfileRowTypes.PERSON:
                        if (columnCount != EXPECTED_COLUMNS_PERSON) {
                            _errorLogs.Add($"Unexpected number of columns for entity at index {rowIndex}, expected 3: {entity}");
                            skipRow = true;
                        } 
                        else
                        {
                            if (isFirstColumn)
                            {
                                var human = CreatePerson();
                                _currentHuman = human;
                            }
                            else
                            {
                                if (columnIndex == 1)
                                    _currentPerson.FirstName = entityValues[columnIndex];
                                else if(columnIndex == 2)
                                    _currentPerson.LastName = entityValues[columnIndex];
                            }
                        }
                        break;
                    case FlatfileRowTypes.FAMILY_MEMBER:
                        if (columnCount != EXPECTED_COLUMNS_FAMILYMEMBER) {
                            _errorLogs.Add($"Unexpected number of columns for entity at index {rowIndex}, expected 3: {entity}");
                            skipRow = true;
                        } 
                        else 
                        {
                            if (isFirstColumn)
                            {
                                var familyMember = AppendNewFamilyMemberToPerson();
                                _currentHuman = familyMember;
                            }
                            else
                            {
                                if (columnIndex == 1)
                                    _currentFamilyMember.FirstName = entityValues[columnIndex];
                                else if(columnIndex == 2)
                                    _currentFamilyMember.BornDate = entityValues[columnIndex];
                            }
                        }
                        break;
                    case FlatfileRowTypes.TELEPHONE:
                        if (columnCount != EXPECTED_COLUMNS_TELEPHONE) {
                            _errorLogs.Add($"Unexpected number of columns for entity at index {rowIndex}, expected 3: {entity}");
                            skipRow = true;
                        }
                        else 
                        {
                            if (isFirstColumn)
                            {
                                _currentHuman.Phone = new Phone();
                            }
                            else
                            {
                                var phone = _currentHuman.Phone;
                                if (columnIndex == 1)
                                    phone.Mobile = entityValues[columnIndex];
                                else if(columnIndex == 2)
                                    phone.Other = entityValues[columnIndex];
                            }
                        }
                        break;
                    case FlatfileRowTypes.ADDRESS:
                        if (columnCount != EXPECTED_COLUMNS_ADDRESS) {
                            _errorLogs.Add($"Unexpected number of columns for entity at index {rowIndex}, expected 3: {entity}");
                            skipRow = true;
                        }
                        else 
                        {
                            if (isFirstColumn)
                            {
                                _currentHuman.Address = new Address();
                            }
                            else
                            {
                                var address = _currentHuman.Address;
                                if (columnIndex == 1)
                                    address.StreetName = entityValues[columnIndex];
                                else if(columnIndex == 2)
                                    address.Town = entityValues[columnIndex];
                                else if(columnIndex == 3)
                                    address.ZipCode = entityValues[columnIndex];
                            }
                        }

                        break;
                    default:
                        _errorLogs.Add($"Unexpected row-type for entity at index {rowIndex}, expected A, T, F or P: {entity}");
                        break;
                }
            }

            rowIndex++;
        }

        return _persons;
    }
}